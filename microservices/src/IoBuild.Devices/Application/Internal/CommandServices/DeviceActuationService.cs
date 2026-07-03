using System.Text.Json;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Infrastructure.Mqtt;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Shared.Domain.Model;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Application.Internal.CommandServices;

/// <summary>
/// Implements <see cref="IDeviceActuationService"/> — the owner-side device command handler.
///
/// Flow (ADR-B2):
///   1. Role guard: only "Owner" allowed.
///   2. Device lookup (404 if missing).
///   3. Permission: device.UnitId must match a UnitOwnerProjection row for requestingUserId.
///   4. Catalog validation: attribute must exist for device.Type; numeric value must be in [Min,Max].
///   5. DB transaction: upsert DeviceShadow + insert DeviceCommandIssuedEvent outbox row → SaveChanges.
///   6. Post-commit: IMqttPublisher.EnqueueAsync (fire-and-forget safe; shadow is durable truth).
///
/// Failure handling (ADR-B2):
///   - SaveChanges failure → 500 (propagated as exception); nothing published, no shadow.
///   - Commit OK but EnqueueAsync fails → shadow already persisted; publisher retry from Channel;
///     QoS1+retain ensures reconnecting device reconciles from retained message.
/// </summary>
public class DeviceActuationService(
    DevicesDbContext db,
    IMqttPublisher mqttPublisher,
    ILogger<DeviceActuationService> logger,
    IDeviceTypeRepository deviceTypeRepository) : IDeviceActuationService
{
    public async Task<ActuationResult> Handle(SendDeviceCommandCommand command)
    {
        // ── 1. Role guard ──────────────────────────────────────────────────────
        if (!string.Equals(command.RequestingUserRole, "Owner", StringComparison.Ordinal))
        {
            logger.LogWarning(
                "DeviceActuationService: role '{Role}' is not Owner. Returning Forbidden.",
                command.RequestingUserRole);
            return ActuationResult.Forbidden("Only owners may issue device commands.");
        }

        // ── 2. Device lookup ───────────────────────────────────────────────────
        var device = await db.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == command.DeviceId);

        if (device is null)
        {
            return ActuationResult.NotFound($"Device {command.DeviceId} not found.");
        }

        // ── 3. Permission check via UnitOwnerProjection (ADR-B4) ───────────────
        if (device.UnitId is null)
        {
            return ActuationResult.Forbidden("Device is not assigned to any unit.");
        }

        var projection = await db.UnitOwnerProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.UnitId == device.UnitId.Value &&
                p.OwnerUserId == command.RequestingUserId);

        if (projection is null)
        {
            logger.LogInformation(
                "DeviceActuationService: no projection row for UserId={UserId} UnitId={UnitId}. " +
                "Owner either not matched or projection not yet consumed (eventual consistency).",
                command.RequestingUserId, device.UnitId.Value);
            return ActuationResult.Forbidden(
                "You do not own the unit containing this device, or ownership has not yet propagated.");
        }

        // ── 4. Capability resolution: catalog first, then owner custom-type DB ───
        var ownerIdStr = command.RequestingUserId.ToString();
        var controllableAttrs = await ResolveControllable(device.Type, ownerIdStr);

        if (controllableAttrs is null)
        {
            return ActuationResult.BadRequest($"Device type '{device.Type}' has no controllable attributes.");
        }

        var attrDef = controllableAttrs
            .FirstOrDefault(a => string.Equals(a.Name, command.Attribute, StringComparison.Ordinal));

        if (attrDef is null)
        {
            return ActuationResult.BadRequest(
                $"Attribute '{command.Attribute}' is not a valid controllable attribute for device type '{device.Type}'. " +
                $"Valid attributes: {string.Join(", ", controllableAttrs.Select(a => a.Name))}.");
        }

        // Numeric range validation
        if (attrDef.Type == "number" && attrDef.Min.HasValue && attrDef.Max.HasValue)
        {
            if (!TryParseNumeric(command.Value, out var numericValue))
            {
                return ActuationResult.BadRequest(
                    $"Attribute '{command.Attribute}' expects a numeric value.");
            }

            if (numericValue < attrDef.Min.Value || numericValue > attrDef.Max.Value)
            {
                return ActuationResult.BadRequest(
                    $"Value {numericValue} is out of range [{attrDef.Min}, {attrDef.Max}] " +
                    $"for attribute '{command.Attribute}'.");
            }
        }

        // ── 5. DB transaction: upsert shadow + outbox row ─────────────────────
        var acceptedAt = DateTime.UtcNow;

        // Upsert shadow row (insert-or-update), merging the new attribute into existing desired state.
        var existingShadow = await db.DeviceShadows
            .FirstOrDefaultAsync(s => s.DeviceId == command.DeviceId);

        var mergedJson = MergeDesiredJson(existingShadow?.DesiredJson, command.Attribute, command.Value);

        if (existingShadow is null)
        {
            db.DeviceShadows.Add(new DeviceShadow
            {
                DeviceId = command.DeviceId,
                DesiredJson = mergedJson,
                UpdatedByUserId = command.RequestingUserId,
                UpdatedAt = acceptedAt
            });
        }
        else
        {
            existingShadow.DesiredJson = mergedJson;
            existingShadow.UpdatedByUserId = command.RequestingUserId;
            existingShadow.UpdatedAt = acceptedAt;
        }

        var desiredJson = mergedJson;

        // Insert DeviceCommandIssuedEvent outbox row
        var auditEvent = new DeviceCommandIssuedEvent
        {
            DeviceId = command.DeviceId,
            Attribute = command.Attribute,
            Value = command.Value?.ToString() ?? string.Empty,
            UserId = command.RequestingUserId,
        };

        var outboxMessage = new OutboxMessage(nameof(DeviceCommandIssuedEvent),
            JsonSerializer.Serialize(auditEvent))
        {
            EventId = auditEvent.EventId
        };

        db.OutboxMessages.Add(outboxMessage);

        // Single SaveChanges = one DB transaction (ADR-B2)
        await db.SaveChangesAsync();

        logger.LogInformation(
            "DeviceActuationService: command committed for DeviceId={DeviceId} Attribute={Attribute} UserId={UserId}.",
            command.DeviceId, command.Attribute, command.RequestingUserId);

        // ── 6. Post-commit: MQTT publish (fire-and-forget safe) ───────────────
        try
        {
            await mqttPublisher.EnqueueAsync(
                command.DeviceId.ToString(),
                desiredJson);
        }
        catch (Exception ex)
        {
            // Shadow is already persisted — this is the durable truth.
            // The channel publisher will retry; QoS1+retain reconciles reconnecting devices.
            logger.LogWarning(ex,
                "DeviceActuationService: MQTT enqueue failed after commit for DeviceId={DeviceId}. " +
                "Shadow is durable; publish will reconcile via channel retry.",
                command.DeviceId);
        }

        return ActuationResult.Ok(acceptedAt);
    }

    // ── ResolveControllable ────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the controllable attribute list for a device type from the global catalog
    /// (device_types table, seeded via the AddDeviceTypeCatalog migration). Returns null when
    /// the type isn't found (caller returns 400).
    ///
    /// Returns <see cref="DeviceCapabilityCatalog.ControllableAttribute"/> records so the
    /// existing range-validation logic is reused unchanged (design D2).
    /// </summary>
    /// <param name="deviceType">Type code of the device.</param>
    /// <param name="ownerId">Owner's string userId (from JWT, already resolved upstream).</param>
    public async Task<IReadOnlyList<DeviceCapabilityCatalog.ControllableAttribute>?> ResolveControllable(
        string deviceType, string ownerId)
    {
        var catalogEntry = await deviceTypeRepository.FindByCodeAsync(deviceType);
        return catalogEntry?.GetAttributes();
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Merges one attribute into the existing desired JSON dict.
    /// Existing attributes are preserved; the new one is added or updated.
    /// </summary>
    private static string MergeDesiredJson(string? existingJson, string attribute, object? value)
    {
        // Parse existing state (or start empty)
        var dict = new Dictionary<string, JsonElement>();
        if (!string.IsNullOrWhiteSpace(existingJson) && existingJson != "{}")
        {
            dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existingJson)
                   ?? new Dictionary<string, JsonElement>();
        }

        // Upsert the new attribute as a JsonElement
        JsonElement newElem;
        if (value is JsonElement je)
        {
            newElem = je;
        }
        else
        {
            var raw = JsonSerializer.SerializeToElement(value);
            newElem = raw;
        }

        dict[attribute] = newElem;

        return JsonSerializer.Serialize(dict);
    }

    private static bool TryParseNumeric(object? value, out double result)
    {
        result = 0;
        if (value is null) return false;

        if (value is JsonElement elem)
        {
            if (elem.ValueKind == JsonValueKind.Number)
            {
                result = elem.GetDouble();
                return true;
            }
            return false;
        }

        return double.TryParse(value.ToString(), out result);
    }
}
