using System.Text;
using System.Text.Json;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IoBuild.Devices.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes <see cref="UnitDevicesDefinedEvent"/> from RabbitMQ
/// and provisions the selected device types per unit (ADR-2/D2, T-17).
///
/// Topology:
///   Exchange : iobuild.domain.events       (topic, durable)
///   Queue    : devices.unit-provisioning   (durable)
///   Binding  : project.unit.devices.defined
///
/// Idempotency (ADR-7-unit):
///   Pre-check: count existing devices for (ProjectId, UnitId) — fast path.
///   Hard backstop: filtered unique index (ProjectId, UnitId, Type) WHERE unit_id IS NOT NULL
///   DbUpdateException from unique violation → treated as "already provisioned" → BasicAck.
///
/// Two-phase commit (ADR-A):
///   Phase 1: SaveChangesAsync() → device rows persisted, real Ids assigned.
///   Phase 2: build DeviceCreatedEvent outbox rows with real device.Id, UnitId, FloorNumber,
///            then SaveChangesAsync().
///
/// DI scope:
///   Singleton BackgroundService. Per-message DI scope resolves DevicesDbContext in production.
///   Test constructor bypasses broker and receives a direct DevicesDbContext (mirrors FloorProvisioningConsumer).
/// </summary>
public class UnitDeviceProvisioningConsumer : BackgroundService
{
    private const string ExchangeName = "iobuild.domain.events";
    private const string QueueName = "devices.unit-provisioning";
    private const string RoutingKey = UnitDevicesDefinedEvent.RoutingKeyValue;

    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly DevicesDbContext? _directDb;   // test seam only
    private readonly ILogger<UnitDeviceProvisioningConsumer> _logger;
    private readonly string? _connectionString;

    /// <summary>Production constructor — resolves DbContext from DI scope per message.</summary>
    public UnitDeviceProvisioningConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<UnitDeviceProvisioningConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _connectionString = configuration["RabbitMq:ConnectionString"];
    }

    /// <summary>
    /// Test constructor — uses a directly-injected DbContext (no broker connection).
    /// Tests call <see cref="ProvisionUnitAsync(UnitDevicesDefinedEvent)"/> directly.
    /// </summary>
    internal UnitDeviceProvisioningConsumer(
        DevicesDbContext db,
        ILogger<UnitDeviceProvisioningConsumer> logger)
    {
        _directDb = db;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning(
                "UnitDeviceProvisioningConsumer: RabbitMq:ConnectionString not configured — consumer disabled.");
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

        IConnection? connection = null;
        IChannel? channel = null;

        try
        {
            connection = await factory.CreateConnectionAsync(stoppingToken);
            channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 5, global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                await HandleDeliveryAsync(channel, ea, stoppingToken);
            };

            await channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "UnitDeviceProvisioningConsumer started. Listening on queue '{Queue}'", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnitDeviceProvisioningConsumer: fatal error in consumer loop");
        }
        finally
        {
            if (channel is not null) await channel.CloseAsync();
            if (connection is not null) await connection.CloseAsync();
        }
    }

    private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken ct)
    {
        ulong deliveryTag = ea.DeliveryTag;

        try
        {
            if (!ea.BasicProperties.Headers!.TryGetValue("event-type", out var rawType))
            {
                _logger.LogWarning("UnitDeviceProvisioningConsumer: missing event-type header. Nacking without requeue.");
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            var eventType = rawType is byte[] bytes ? Encoding.UTF8.GetString(bytes) : rawType?.ToString();

            if (eventType != nameof(UnitDevicesDefinedEvent))
            {
                _logger.LogWarning(
                    "UnitDeviceProvisioningConsumer: unexpected EventType={EventType} on {Queue}. Nacking without requeue.",
                    eventType, QueueName);
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            var evt = JsonSerializer.Deserialize<UnitDevicesDefinedEvent>(ea.Body.ToArray())
                ?? throw new JsonException("Failed to deserialize UnitDevicesDefinedEvent");

            if (_scopeFactory is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
                await ProvisionUnitAsync(evt, db);
            }
            else
            {
                await ProvisionUnitAsync(evt, _directDb!);
            }

            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (DbUpdateException dbEx) when (IsUniqueConstraintViolation(dbEx))
        {
            _logger.LogWarning(
                "UnitDeviceProvisioningConsumer: unique-constraint violation on insert — " +
                "unit already provisioned. Acking as already-done.");
            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (Exception ex) when (IsTransientError(ex))
        {
            _logger.LogWarning(ex, "UnitDeviceProvisioningConsumer: transient error. Nacking with requeue.");
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnitDeviceProvisioningConsumer: poison message. Nacking without requeue.");
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
        }
    }

    /// <summary>
    /// Core provisioning logic — called both from the broker handler (production) and directly in tests.
    /// Implements idempotency pre-check + two-phase commit + outbox write (ADR-A, ADR-7-unit).
    /// </summary>
    internal async Task ProvisionUnitAsync(UnitDevicesDefinedEvent evt)
    {
        if (_directDb is null)
            throw new InvalidOperationException(
                "ProvisionUnitAsync(event) without db is only valid when _directDb is set (test path).");
        await ProvisionUnitAsync(evt, _directDb);
    }

    private async Task ProvisionUnitAsync(UnitDevicesDefinedEvent evt, DevicesDbContext db)
    {
        // Resolve distinct selected types — duplicates dropped at the consumer level too.
        var selected = evt.DeviceTypes.Distinct().ToList();

        if (selected.Count == 0)
        {
            _logger.LogInformation(
                "UnitDeviceProvisioningConsumer: empty package for UnitId={UnitId} ProjectId={ProjectId} — skipping.",
                evt.UnitId, evt.ProjectId);
            return;
        }

        // Idempotency pre-check (count-based, mirrors FloorProvisioningConsumer — ADR-7-unit).
        // If existing device count for (ProjectId, UnitId) >= selected count, unit is already
        // provisioned → skip. The filtered unique index is the hard backstop for racing redelivery.
        var existingCount = await db.Devices.CountAsync(
            d => d.ProjectId == evt.ProjectId && d.UnitId == evt.UnitId);

        if (existingCount >= selected.Count)
        {
            _logger.LogInformation(
                "UnitDeviceProvisioningConsumer: unit {UnitId} of project {ProjectId} already provisioned " +
                "(existingCount={Existing} >= expectedCount={Expected}) — skipping.",
                evt.UnitId, evt.ProjectId, existingCount, selected.Count);
            return;
        }

        // Resolve display names from the DB catalog; fall back to the type code itself if the
        // type is not found (consumer is tolerant of future additions). The catalog is read once
        // into a dictionary and resolved in memory — no per-type DB round-trips.
        // Slice 2 repoint — design D5; UnitDeviceCatalog static class deleted.
        var nameByCode = await db.DeviceTypes.ToDictionaryAsync(dt => dt.Code, dt => dt.DisplayName);

        var devices = new List<Device>();
        foreach (var type in selected)
        {
            var displayName = nameByCode.GetValueOrDefault(type, type);

            var mac = GenerateMac(evt.ProjectId, evt.UnitId, type);
            var device = Device.ForUnitPackage(
                name: $"{displayName} - Unit {evt.RoomNumber}",
                type: type,
                location: $"Floor {evt.Floor} - Room {evt.RoomNumber}",
                mac: mac,
                projectId: evt.ProjectId,
                status: "Active",
                floorNumber: evt.Floor,
                unitId: evt.UnitId);

            await db.Devices.AddAsync(device);
            devices.Add(device);
        }

        await db.SaveChangesAsync();   // Phase 1 commit — device.Id is now real

        // Phase 2: build DeviceCreatedEvent outbox rows with real device.Id + UnitId + FloorNumber.
        foreach (var device in devices)
        {
            var domainEvt = new DeviceCreatedEvent
            {
                DeviceId = device.Id,
                OwnerUserId = 0,   // Owner is derived read-side via UnitProjection (D6) — NOT set here
                ProjectId = device.ProjectId,
                UnitId = device.UnitId,
                DeviceType = device.Type,
                Status = device.Status,
                FloorNumber = evt.Floor
            };

            var payload = JsonSerializer.Serialize(domainEvt);
            var outboxMessage = new OutboxMessage(nameof(DeviceCreatedEvent), payload)
            {
                EventId = domainEvt.EventId
            };

            await db.OutboxMessages.AddAsync(outboxMessage);
        }

        await db.SaveChangesAsync();   // Phase 2 commit — outbox rows persisted

        _logger.LogInformation(
            "UnitDeviceProvisioningConsumer: provisioned {Count} devices for ProjectId={ProjectId} UnitId={UnitId}.",
            devices.Count, evt.ProjectId, evt.UnitId);
    }

    /// <summary>
    /// Generates a deterministic MAC address from (projectId, unitId, deviceType).
    /// Uses "U1:" prefix to distinguish from floor-provisioned "F1:" MACs (ADR-3/D3).
    /// Including UnitId in the hash avoids collision when two units on the same floor
    /// have the same device type (the critical scenario — ADR-7-unit).
    /// </summary>
    private static string GenerateMac(int projectId, int unitId, string type)
    {
        var hash = HashCode.Combine(projectId, unitId, type.GetHashCode());
        var bytes = BitConverter.GetBytes(Math.Abs(hash));
        return $"U1:{bytes[0]:X2}:{bytes[1]:X2}:{bytes[2]:X2}:{bytes[3]:X2}:{Math.Abs(type.GetHashCode() & 0xFF):X2}";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsTransientError(Exception ex) =>
        ex is DbUpdateException or TimeoutException;
}
