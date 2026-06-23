using System.Text.Json;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Domain.Model.Exceptions;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Application.Internal.CommandServices;

/// <summary>
/// Handles device creation, update, and deletion commands.
///
/// Extended for the owner-custom-device-type feature:
///   - When CreateDeviceCommand.UnitId is set, creates the device via Device.ForOwnerCustom.
///   - Validates the requesting owner owns the unit (via UnitOwnerProjection).
///   - Validates the typeCode belongs to the requesting owner (via OwnerCustomDeviceType).
///   - Catches DbUpdateException on the composite unique index violation → throws
///     DuplicateDeviceTypeOnUnitException (controller maps to 409).
///   - Emits DeviceName in DeviceCreatedEvent so Analytics can surface the user-given name.
/// </summary>
public class DeviceCommandService(
    IDeviceRepository repository,
    IOutboxMessageRepository outboxRepository,
    DevicesDbContext? db = null) : IDeviceCommandService
{
    public async Task<Device> Handle(CreateDeviceCommand command)
    {
        Device device;

        if (command.UnitId.HasValue)
        {
            // ── Owner-custom path ──────────────────────────────────────────────
            if (db is null)
                throw new InvalidOperationException(
                    "DevicesDbContext is required for owner-custom device creation.");

            var ownerId = command.RequestingOwnerId
                ?? throw new InvalidOperationException("RequestingOwnerId is required when UnitId is set.");

            // Validate calling user owns the unit
            var unitOwnership = await db.UnitOwnerProjections
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.UnitId == command.UnitId.Value &&
                    p.OwnerUserId == int.Parse(ownerId));

            if (unitOwnership is null)
                throw new UnauthorizedAccessException(
                    $"You do not own unit {command.UnitId.Value} or ownership has not yet propagated.");

            // Validate typeCode belongs to the requesting owner
            var customType = await db.OwnerCustomDeviceTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.TypeCode == command.Type &&
                    t.OwnerUserId == ownerId);

            if (customType is null)
                throw new UnauthorizedAccessException(
                    $"Device type '{command.Type}' does not belong to your account.");

            // Pre-check: OwnerCustom devices with same type on same unit (NULL floor_number
            // means the composite unique index won't fire in MySQL/SQLite due to NULL distinctness,
            // so we do an explicit existence check before inserting).
            var existingDevice = await db.Devices
                .AsNoTracking()
                .AnyAsync(d =>
                    d.ProjectId == command.ProjectId &&
                    d.UnitId == command.UnitId.Value &&
                    d.Type == command.Type &&
                    d.Source == "OwnerCustom");

            if (existingDevice)
                throw new DuplicateDeviceTypeOnUnitException(
                    "A device of this type already exists in this unit.");

            device = Device.ForOwnerCustom(
                command.Name, command.Type, command.Location,
                command.MacAddress, command.ProjectId, command.Status,
                command.UnitId.Value);
        }
        else
        {
            // ── Standard manual-creation path (unchanged) ──────────────────────
            device = new Device(
                command.Name, command.Type, command.Location,
                command.MacAddress, command.ProjectId, command.Status);
        }

        await repository.AddAsync(device);

        // Phase 1: persist device → EF/MySQL assigns real identity
        try
        {
            await repository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicateTypeOnUnit(ex))
        {
            throw new DuplicateDeviceTypeOnUnitException(
                "A device of this type already exists in this unit.");
        }

        // Phase 2: build event with real device.Id + DeviceName, persist outbox row
        var evt = new DeviceCreatedEvent
        {
            DeviceId   = device.Id,
            OwnerUserId = 0,
            ProjectId  = device.ProjectId,
            DeviceType = device.Type,
            Status     = device.Status,
            FloorNumber = device.FloorNumber,
            UnitId     = device.UnitId,
            DeviceName = device.Name   // carries user-given name for OwnerCustom devices
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(DeviceCreatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await outboxRepository.AddAsync(outboxMessage);
        await repository.SaveChangesAsync();

        return device;
    }

    public async Task<Device> Handle(UpdateDeviceCommand command)
    {
        var device = await repository.FindByIdAsync(command.Id);

        if (device is null)
            throw new KeyNotFoundException($"Device with id {command.Id} not found.");

        device.Update(
            command.Name,
            command.Type,
            command.Location,
            command.MacAddress,
            command.ProjectId,
            command.Status);

        repository.Update(device);

        var evt = new DeviceUpdatedEvent
        {
            DeviceId = device.Id,
            OwnerUserId = 0,
            ProjectId = device.ProjectId,
            DeviceType = device.Type,
            Status = device.Status
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(DeviceUpdatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await outboxRepository.AddAsync(outboxMessage);
        await repository.SaveChangesAsync();

        return device;
    }

    public async Task Handle(DeleteDeviceCommand command)
    {
        var device = await repository.FindByIdAsync(command.Id);

        if (device is null)
            throw new KeyNotFoundException($"Device with id {command.Id} not found.");

        var evt = new DeviceDeletedEvent
        {
            DeviceId = device.Id,
            OwnerUserId = 0
        };

        repository.Remove(device);

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(DeviceDeletedEvent), payload)
        {
            EventId = evt.EventId
        };

        await outboxRepository.AddAsync(outboxMessage);
        await repository.SaveChangesAsync();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Detects whether a DbUpdateException is caused by the composite unique index on the
    /// Device table (project_id, floor_number, unit_key, type), which fires when an owner
    /// tries to add a second device of the same custom type to the same unit.
    /// </summary>
    private static bool IsDuplicateTypeOnUnit(DbUpdateException ex)
    {
        // SQLite: "UNIQUE constraint failed" in message
        // MySQL:  "Duplicate entry" in inner exception message
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase);
    }
}
