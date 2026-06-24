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
/// Background service that consumes <see cref="FloorStructureDefinedEvent"/> from RabbitMQ
/// and provisions the default device set per floor (REQ-FD-01..FD-04, §4.1–4.3).
///
/// Topology:
///   Exchange : iobuild.domain.events (topic, durable)
///   Queue    : devices.provisioning  (durable)
///   Binding  : project.floor.defined
///
/// Idempotency (ADR-C):
///   Pre-check: ExistsByProjectFloorTypeAsync before inserting (fast path — avoids exception on common path).
///   Hard backstop: unique index (ProjectId, FloorNumber, Type) — catches racing concurrent deliveries.
///   DbUpdateException from unique violation → treated as "already provisioned" → BasicAck.
///
/// Two-phase commit (ADR-A):
///   Phase 1: SaveChangesAsync() → device rows persisted, real Ids assigned.
///   Phase 2: build DeviceCreatedEvent outbox rows with real device.Id and FloorNumber, then SaveChangesAsync().
///
/// DI scope:
///   Singleton BackgroundService. Per-message DI scope resolves DevicesDbContext in production.
///   Test constructor bypasses broker and receives a direct DevicesDbContext (mirroring AnalyticsEventConsumer).
/// </summary>
public class FloorProvisioningConsumer : BackgroundService
{
    private const string ExchangeName = "iobuild.domain.events";
    private const string QueueName = "devices.provisioning";
    private const string RoutingKey = "project.floor.defined";

    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly DevicesDbContext? _directDb;   // test seam only
    private readonly ILogger<FloorProvisioningConsumer> _logger;
    private readonly string? _connectionString;

    /// <summary>Production constructor — resolves DbContext from DI scope per message.</summary>
    public FloorProvisioningConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<FloorProvisioningConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _connectionString = configuration["RabbitMq:ConnectionString"];
    }

    /// <summary>
    /// Test constructor — uses a directly-injected DbContext (no broker connection).
    /// Tests call <see cref="ProvisionFloorAsync"/> directly with a pre-built event.
    /// </summary>
    internal FloorProvisioningConsumer(
        DevicesDbContext db,
        ILogger<FloorProvisioningConsumer> logger)
    {
        _directDb = db;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning(
                "FloorProvisioningConsumer: RabbitMq:ConnectionString not configured — consumer disabled.");
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

        IConnection? connection = null;
        IChannel? channel = null;

        try
        {
            connection = await factory.CreateConnectionAsync(stoppingToken);
            channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Declare exchange (idempotent)
            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            // Declare queue (durable)
            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            // Bind to project.floor.defined routing key
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
                "FloorProvisioningConsumer started. Listening on queue '{Queue}'", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FloorProvisioningConsumer: fatal error in consumer loop");
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
            // Read event-type header (guard against non-FloorStructureDefined messages on this queue)
            if (!ea.BasicProperties.Headers!.TryGetValue("event-type", out var rawType))
            {
                _logger.LogWarning("FloorProvisioningConsumer: missing event-type header. Nacking without requeue.");
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            var eventType = rawType is byte[] bytes ? Encoding.UTF8.GetString(bytes) : rawType?.ToString();

            if (eventType != nameof(FloorStructureDefinedEvent))
            {
                _logger.LogWarning(
                    "FloorProvisioningConsumer: unexpected EventType={EventType} on devices.provisioning. Nacking without requeue.",
                    eventType);
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            var evt = JsonSerializer.Deserialize<FloorStructureDefinedEvent>(ea.Body.ToArray())
                ?? throw new JsonException("Failed to deserialize FloorStructureDefinedEvent");

            if (_scopeFactory is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
                await ProvisionFloorAsync(evt, db);
            }
            else
            {
                await ProvisionFloorAsync(evt, _directDb!);
            }

            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (DbUpdateException dbEx) when (IsUniqueConstraintViolation(dbEx))
        {
            // Already provisioned (concurrent redelivery won the race after the pre-check)
            // → treat as success, ack to avoid poison-loop (ADR-C).
            _logger.LogWarning(
                "FloorProvisioningConsumer: unique-constraint violation on insert — " +
                "floor already provisioned. Acking as already-done.");
            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (Exception ex) when (IsTransientError(ex))
        {
            _logger.LogWarning(ex, "FloorProvisioningConsumer: transient error. Nacking with requeue.");
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FloorProvisioningConsumer: poison message. Nacking without requeue.");
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
        }
    }

    /// <summary>
    /// Core provisioning logic — called both from the broker handler (production) and directly in tests.
    /// Implements idempotency pre-check + two-phase commit + outbox write (ADR-A, ADR-C).
    /// </summary>
    internal async Task ProvisionFloorAsync(FloorStructureDefinedEvent evt)
    {
        if (_directDb is null)
            throw new InvalidOperationException(
                "ProvisionFloorAsync(event) without db is only valid when _directDb is set (test path).");
        await ProvisionFloorAsync(evt, _directDb);
    }

    private async Task ProvisionFloorAsync(FloorStructureDefinedEvent evt, DevicesDbContext db)
    {
        // Resolve the set of (Type, NamePrefix) tuples to provision.
        // evt.DeviceTypes is set by the producer (Projects) for selective provisioning (S4.1).
        // null or empty → fall back to all floor-scoped types from the DB catalog (S4.2, S4.3, ADR-4).
        // Display names come from the DB catalog (Slice 2 repoint — design D5). The catalog is
        // small (5 rows) and read once here, then resolved in memory — no per-type DB round-trips.
        var catalog = await db.DeviceTypes
            .OrderBy(dt => dt.Id)
            .ToListAsync();

        List<(string Type, string NamePrefix)> selected;
        if (evt.DeviceTypes is { Count: > 0 })
        {
            var nameByCode = catalog.ToDictionary(dt => dt.Code, dt => dt.DisplayName);
            // Resolve display name from the catalog by Code; fall back to the type code itself
            // if the type is not found (consumer is tolerant of future additions).
            selected = evt.DeviceTypes
                .Select(t => (Type: t, NamePrefix: nameByCode.GetValueOrDefault(t, t)))
                .ToList();
        }
        else
        {
            // Default fallback: all floor-scoped types from the DB catalog, ordered by Id.
            // Yields exactly SmartMeter, WaterSensor, SmokeDetector — behavior preserved.
            selected = catalog
                .Where(dt => dt.Scope == "floor")
                .Select(dt => (Type: dt.Code, NamePrefix: dt.DisplayName))
                .ToList();
        }

        // Idempotency pre-check (count-based, replaces SmartMeter sentinel — ADR-7 / SC-4.4).
        // If existing device count >= selected count, floor is already provisioned → skip.
        // The unique index (ProjectId, FloorNumber, Type) remains the hard backstop for
        // concurrent redelivery that sneaks past this pre-check (ADR-C).
        var existingCount = await db.Devices.CountAsync(
            d => d.ProjectId == evt.ProjectId && d.FloorNumber == evt.Floor);

        if (existingCount >= selected.Count)
        {
            _logger.LogInformation(
                "FloorProvisioningConsumer: floor {Floor} of project {ProjectId} already provisioned " +
                "(existingCount={Existing} >= expectedCount={Expected}) — skipping.",
                evt.Floor, evt.ProjectId, existingCount, selected.Count);
            return;
        }

        // Phase 1: create and persist device rows → real Ids assigned by DB (ADR-A).
        // Each device uses the floor-provisioned constructor so Source="FloorProvisioned" (S5.1).
        var devices = new List<Device>();
        foreach (var (type, namePrefix) in selected)
        {
            var mac = GenerateMac(evt.ProjectId, evt.Floor, type);
            var device = new Device(
                name: $"{namePrefix} - Floor {evt.Floor}",
                type: type,
                location: $"Floor {evt.Floor}",
                macAddress: mac,
                projectId: evt.ProjectId,
                status: "Active",
                floorNumber: evt.Floor);

            await db.Devices.AddAsync(device);
            devices.Add(device);
        }

        await db.SaveChangesAsync();   // Phase 1 commit — device.Id is now real

        // Phase 2: build DeviceCreatedEvent outbox rows with the real device.Id and FloorNumber.
        foreach (var device in devices)
        {
            var domainEvt = new DeviceCreatedEvent
            {
                DeviceId = device.Id,
                OwnerUserId = 0,
                ProjectId = device.ProjectId,
                DeviceType = device.Type,
                Status = device.Status,
                FloorNumber = evt.Floor   // REQ-FD-02: must equal the triggering event's Floor
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
            "FloorProvisioningConsumer: provisioned {Count} devices for ProjectId={ProjectId} Floor={Floor}.",
            devices.Count, evt.ProjectId, evt.Floor);
    }

    /// <summary>
    /// Generates a deterministic MAC address from (projectId, floor, deviceType).
    /// Keeps the format XX:XX:XX:XX:XX:XX and avoids DB unique-constraint collisions across floors.
    /// </summary>
    private static string GenerateMac(int projectId, int floor, string type)
    {
        // Simple deterministic hash — good enough for seed/test; real hardware uses DHCP / provisioning.
        var hash = HashCode.Combine(projectId, floor, type.GetHashCode());
        var bytes = BitConverter.GetBytes(Math.Abs(hash));
        // Pad/truncate to 5 bytes and prefix with "F1" to mark as floor-provisioned
        return $"F1:{bytes[0]:X2}:{bytes[1]:X2}:{bytes[2]:X2}:{bytes[3]:X2}:{Math.Abs(type.GetHashCode() & 0xFF):X2}";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsTransientError(Exception ex) =>
        ex is DbUpdateException or TimeoutException;
}
