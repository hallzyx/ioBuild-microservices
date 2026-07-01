using System.Text;
using System.Text.Json;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IoBuild.Analytics.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes domain events from RabbitMQ and maintains
/// the Analytics read-model projection tables (ADR-3, ADR-5, REQ-RM-02..RM-05).
///
/// Topology (ADR-3):
///   Exchange : iobuild.domain.events (topic, durable)
///   Queue    : analytics.read-model  (durable)
///   Bindings : device.# and project.# routing-key prefixes
///
/// Idempotency (ADR-5):
///   *Created / *Updated → upsert by natural key with last_event_at LWW guard.
///   *Deleted            → delete-if-exists (no-op when row absent).
///
/// Ack semantics (ADR-5):
///   Success          → BasicAck
///   Transient DB err → BasicNack(requeue:true)
///   Poison message   → log + BasicNack(requeue:false)
///
/// DI scope (mirrors TelemetryWorker pattern):
///   This class is registered as a singleton BackgroundService.
///   A DI scope is opened per message delivery to resolve the scoped DbContext.
///   When constructed directly (e.g. from unit tests), the caller passes the DbContext directly.
/// </summary>
public class AnalyticsEventConsumer : BackgroundService
{
    private const string ExchangeName = "iobuild.domain.events";
    private const string QueueName = "analytics.read-model";

    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly AnalyticsDbContext? _directDb;   // used in tests only
    private readonly ILogger<AnalyticsEventConsumer> _logger;
    private readonly string? _connectionString;

    /// <summary>
    /// Production constructor — resolves DbContext from DI scope per message.
    /// </summary>
    public AnalyticsEventConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AnalyticsEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _connectionString = configuration["RabbitMq:ConnectionString"];
    }

    /// <summary>
    /// Test constructor — uses a directly-injected DbContext (no broker connection).
    /// </summary>
    internal AnalyticsEventConsumer(
        AnalyticsDbContext db,
        ILogger<AnalyticsEventConsumer> logger)
    {
        _directDb = db;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning(
                "AnalyticsEventConsumer: RabbitMq:ConnectionString is not configured — consumer is disabled.");
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

        while (!stoppingToken.IsCancellationRequested)
        {
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

                // Bindings (ADR-3: device.# and project.# cover Unit events too)
                await channel.QueueBindAsync(QueueName, ExchangeName, "device.#", cancellationToken: stoppingToken);
                await channel.QueueBindAsync(QueueName, ExchangeName, "project.#", cancellationToken: stoppingToken);

                // Manual ack
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

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

                _logger.LogInformation("AnalyticsEventConsumer started. Listening on queue '{Queue}'", QueueName);

                // Keep alive until cancellation or broker disconnect
                await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown — exit the loop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyticsEventConsumer: broker connection lost — reconnecting in 10s");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            finally
            {
                if (channel is not null) try { await channel.CloseAsync(); } catch { }
                if (connection is not null) try { await connection.CloseAsync(); } catch { }
            }
        }
    }

    private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken ct)
    {
        ulong deliveryTag = ea.DeliveryTag;
        string? eventType = null;

        try
        {
            // Read event-type header
            if (!ea.BasicProperties.Headers!.TryGetValue("event-type", out var rawType))
            {
                _logger.LogWarning("AnalyticsEventConsumer: missing event-type header. Nacking without requeue.");
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            eventType = rawType is byte[] bytes
                ? Encoding.UTF8.GetString(bytes)
                : rawType?.ToString();

            var body = ea.Body.ToArray();

            // Open a DI scope per message to get a fresh DbContext (production path).
            // The test path (_directDb != null) never reaches HandleDeliveryAsync —
            // tests call ApplyEventAsync directly with a pre-wired DbContext.
            if (_scopeFactory is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
                await ApplyEventByTypeAsync(eventType!, body, db, ct);
            }
            else
            {
                // Defensive: should not occur in practice (test constructor bypasses this method)
                await ApplyEventByTypeAsync(eventType!, body, _directDb!, ct);
            }

            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (Exception ex) when (IsTransientDbError(ex))
        {
            _logger.LogWarning(ex,
                "AnalyticsEventConsumer: transient DB error for EventType={EventType}. Nacking with requeue.",
                eventType);
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AnalyticsEventConsumer: poison message for EventType={EventType}. Nacking without requeue.",
                eventType);
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task ApplyEventByTypeAsync(string eventType, byte[] body, AnalyticsDbContext db, CancellationToken ct)
    {
        switch (eventType)
        {
            case nameof(DeviceCreatedEvent):
                await UpsertDeviceAsync(Deserialize<DeviceCreatedEvent>(body), db);
                break;
            case nameof(DeviceUpdatedEvent):
                await UpsertDeviceAsync(Deserialize<DeviceUpdatedEvent>(body), db);
                break;
            case nameof(DeviceDeletedEvent):
                await DeleteDeviceAsync(Deserialize<DeviceDeletedEvent>(body), db);
                break;
            case nameof(ProjectCreatedEvent):
                await UpsertProjectAsync(Deserialize<ProjectCreatedEvent>(body), db);
                break;
            case nameof(ProjectUpdatedEvent):
                await UpsertProjectAsync(Deserialize<ProjectUpdatedEvent>(body), db);
                break;
            case nameof(UnitCreatedEvent):
                await UpsertUnitAsync(Deserialize<UnitCreatedEvent>(body), db);
                break;
            case nameof(UnitOwnerMatchedEvent):
                await UpsertUnitOwnerAsync(Deserialize<UnitOwnerMatchedEvent>(body), db);
                break;
            default:
                _logger.LogWarning(
                    "AnalyticsEventConsumer: unknown EventType={EventType}. Nacking without requeue.",
                    eventType);
                throw new InvalidOperationException($"Unknown event type: {eventType}");
        }
    }

    // ── Public for unit tests ──

    /// <summary>
    /// Applies a domain event to the projection tables.
    /// Tests using the internal constructor call this directly with a pre-wired DbContext.
    /// Production path reaches here only through HandleDeliveryAsync which opens a DI scope.
    /// </summary>
    public async Task ApplyEventAsync(DomainEvent evt)
    {
        // Resolve db: test constructor sets _directDb; production constructor uses _scopeFactory.
        // When called from tests (internal constructor), _directDb is always set.
        // When called from production tests using the production constructor, we open a scope.
        if (_directDb is not null)
        {
            await ApplyEventWithDb(evt, _directDb);
            return;
        }

        if (_scopeFactory is not null)
        {
            // Keep the scope alive for the entire await so the DbContext is not disposed
            // before the async DB operation completes (WARNING-A fix).
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            await ApplyEventWithDb(evt, db);
            return;
        }

        throw new InvalidOperationException(
            "AnalyticsEventConsumer: neither _directDb nor _scopeFactory is set. This should never happen.");
    }

    private Task ApplyEventWithDb(DomainEvent evt, AnalyticsDbContext db) => evt switch
    {
        DeviceCreatedEvent e      => UpsertDeviceAsync(e, db),
        DeviceUpdatedEvent e      => UpsertDeviceAsync(e, db),
        DeviceDeletedEvent e      => DeleteDeviceAsync(e, db),
        ProjectCreatedEvent e     => UpsertProjectAsync(e, db),
        ProjectUpdatedEvent e     => UpsertProjectAsync(e, db),
        UnitCreatedEvent e        => UpsertUnitAsync(e, db),
        UnitOwnerMatchedEvent e   => UpsertUnitOwnerAsync(e, db),
        _ => throw new InvalidOperationException($"Unsupported event type: {evt.GetType().Name}")
    };

    // ── Projection upsert / delete logic ──

    private async Task UpsertDeviceAsync(DeviceCreatedEvent evt, AnalyticsDbContext db)
    {
        var row = await db.DeviceProjections.FindAsync(evt.DeviceId);
        if (row is null)
        {
            row = new DeviceProjection { DeviceId = evt.DeviceId };
            db.DeviceProjections.Add(row);
        }
        else if (evt.OccurredOn < row.LastEventAt)
        {
            // LWW guard — stale event, discard
            return;
        }

        row.OwnerUserId  = evt.OwnerUserId;
        row.ProjectId    = evt.ProjectId;
        row.UnitId       = evt.UnitId;
        row.DeviceType   = evt.DeviceType;
        row.Status       = evt.Status;
        row.FloorNumber  = evt.FloorNumber;   // PR 7 — §5.2
        row.DeviceName   = evt.DeviceName;    // owner-custom-device-type: null for legacy events
        row.LastEventAt  = evt.OccurredOn;
        await db.SaveChangesAsync();
    }

    private async Task UpsertDeviceAsync(DeviceUpdatedEvent evt, AnalyticsDbContext db)
    {
        var row = await db.DeviceProjections.FindAsync(evt.DeviceId);
        if (row is null)
        {
            row = new DeviceProjection { DeviceId = evt.DeviceId };
            db.DeviceProjections.Add(row);
        }
        else if (evt.OccurredOn < row.LastEventAt)
        {
            return;
        }

        row.OwnerUserId = evt.OwnerUserId;
        row.ProjectId   = evt.ProjectId;
        row.UnitId      = evt.UnitId;
        row.DeviceType  = evt.DeviceType;
        row.Status      = evt.Status;
        row.LastEventAt = evt.OccurredOn;
        await db.SaveChangesAsync();
    }

    private async Task DeleteDeviceAsync(DeviceDeletedEvent evt, AnalyticsDbContext db)
    {
        var row = await db.DeviceProjections.FindAsync(evt.DeviceId);
        if (row is not null)
        {
            db.DeviceProjections.Remove(row);
            await db.SaveChangesAsync();
        }
    }

    private async Task UpsertProjectAsync(ProjectCreatedEvent evt, AnalyticsDbContext db)
    {
        var row = await db.ProjectProjections.FindAsync(evt.ProjectId);
        if (row is null)
        {
            row = new ProjectProjection { ProjectId = evt.ProjectId };
            db.ProjectProjections.Add(row);
        }
        else if (evt.OccurredOn < row.LastEventAt)
        {
            return;
        }

        row.BuilderUserId = evt.BuilderUserId;
        row.Name          = evt.Name;
        row.Status        = evt.Status;
        row.LastEventAt   = evt.OccurredOn;
        await db.SaveChangesAsync();
    }

    private async Task UpsertProjectAsync(ProjectUpdatedEvent evt, AnalyticsDbContext db)
    {
        var row = await db.ProjectProjections.FindAsync(evt.ProjectId);
        if (row is null)
        {
            row = new ProjectProjection { ProjectId = evt.ProjectId };
            db.ProjectProjections.Add(row);
        }
        else if (evt.OccurredOn < row.LastEventAt)
        {
            return;
        }

        row.BuilderUserId = evt.BuilderUserId;
        row.Name          = evt.Name;
        row.Status        = evt.Status;
        row.LastEventAt   = evt.OccurredOn;
        await db.SaveChangesAsync();
    }

    private async Task UpsertUnitAsync(UnitCreatedEvent evt, AnalyticsDbContext db)
    {
        var row = await db.UnitProjections.FindAsync(evt.UnitId);
        if (row is null)
        {
            row = new UnitProjection { UnitId = evt.UnitId };
            db.UnitProjections.Add(row);
        }
        else if (evt.OccurredOn < row.LastEventAt)
        {
            return;
        }

        row.ProjectId     = evt.ProjectId;
        row.BuilderUserId = evt.BuilderUserId;
        // LWW: only overwrite OwnerUserId if the event carries a non-null value.
        // Preserves a value already set by UnitOwnerMatchedEvent in the out-of-order scenario.
        if (evt.OwnerUserId.HasValue)
            row.OwnerUserId = evt.OwnerUserId;
        row.Status        = evt.Status;
        row.Floor         = evt.Floor;         // PR 7 — §5.2
        row.RoomNumber    = evt.RoomNumber;    // PR 7 — §5.2
        row.OwnerEmail    = evt.OwnerEmail;    // PR 7 — §5.2
        row.LastEventAt   = evt.OccurredOn;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Handles UnitOwnerMatchedEvent — sets OwnerUserId on the projection.
    /// Creates a placeholder row if the projection does not exist yet (out-of-order delivery).
    /// LWW guard on OccurredOn.
    /// </summary>
    private async Task UpsertUnitOwnerAsync(UnitOwnerMatchedEvent evt, AnalyticsDbContext db)
    {
        var row = await db.UnitProjections.FindAsync(evt.UnitId);
        if (row is null)
        {
            // Out-of-order: projection not yet created — create a placeholder.
            row = new UnitProjection
            {
                UnitId     = evt.UnitId,
                ProjectId  = evt.ProjectId,
                Status     = string.Empty,
                LastEventAt = DateTime.MinValue   // placeholder; will be updated below
            };
            db.UnitProjections.Add(row);
        }
        else if (evt.OccurredOn < row.LastEventAt)
        {
            // LWW guard — stale event, discard
            return;
        }

        row.OwnerUserId = evt.OwnerUserId;
        if (!string.IsNullOrEmpty(evt.OwnerEmail))
            row.OwnerEmail = evt.OwnerEmail;
        row.Status      = "Occupied";
        row.LastEventAt = evt.OccurredOn;

        // Propagate owner link to device projections on the same floor so that
        // the owner dashboard query (WHERE u.unit_id = d.unit_id) can join correctly.
        if (row.Floor > 0)
        {
            var floorDevices = await db.DeviceProjections
                .Where(d => d.ProjectId == evt.ProjectId && d.FloorNumber == row.Floor)
                .ToListAsync();
            foreach (var d in floorDevices)
            {
                d.UnitId      = evt.UnitId;
                d.OwnerUserId = evt.OwnerUserId;
            }
        }

        await db.SaveChangesAsync();
    }

    private static T Deserialize<T>(byte[] body) =>
        JsonSerializer.Deserialize<T>(body)
        ?? throw new JsonException($"Failed to deserialize {typeof(T).Name} — result was null.");

    private static bool IsTransientDbError(Exception ex) =>
        ex is DbUpdateException or TimeoutException;
}
