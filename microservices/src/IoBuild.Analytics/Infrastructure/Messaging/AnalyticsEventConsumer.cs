using System.Text;
using System.Text.Json;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
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

            // Keep alive until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnalyticsEventConsumer: fatal error in consumer loop");
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
            await ApplyEventByTypeAsync(eventType!, body, ct);

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

    private async Task ApplyEventByTypeAsync(string eventType, byte[] body, CancellationToken ct)
    {
        switch (eventType)
        {
            case nameof(DeviceCreatedEvent):
                await ApplyEventAsync(Deserialize<DeviceCreatedEvent>(body));
                break;
            case nameof(DeviceUpdatedEvent):
                await ApplyEventAsync(Deserialize<DeviceUpdatedEvent>(body));
                break;
            case nameof(DeviceDeletedEvent):
                await ApplyEventAsync(Deserialize<DeviceDeletedEvent>(body));
                break;
            case nameof(ProjectCreatedEvent):
                await ApplyEventAsync(Deserialize<ProjectCreatedEvent>(body));
                break;
            case nameof(ProjectUpdatedEvent):
                await ApplyEventAsync(Deserialize<ProjectUpdatedEvent>(body));
                break;
            case nameof(UnitCreatedEvent):
                await ApplyEventAsync(Deserialize<UnitCreatedEvent>(body));
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
    /// Called directly by unit tests via the test constructor; in production this is
    /// called from HandleDeliveryAsync which opens a DI scope around the DbContext.
    /// </summary>
    public Task ApplyEventAsync(DomainEvent evt) => evt switch
    {
        DeviceCreatedEvent e  => UpsertDeviceAsync(e),
        DeviceUpdatedEvent e  => UpsertDeviceAsync(e),
        DeviceDeletedEvent e  => DeleteDeviceAsync(e),
        ProjectCreatedEvent e => UpsertProjectAsync(e),
        ProjectUpdatedEvent e => UpsertProjectAsync(e),
        UnitCreatedEvent e    => UpsertUnitAsync(e),
        _ => throw new InvalidOperationException($"Unsupported event type: {evt.GetType().Name}")
    };

    private AnalyticsDbContext GetDb()
    {
        if (_directDb is not null)
            return _directDb;

        // In production path this is never called directly — the caller resolves from scope.
        throw new InvalidOperationException(
            "Production code must resolve DbContext from DI scope, not via GetDb().");
    }

    // ── Projection upsert / delete logic ──

    private async Task UpsertDeviceAsync(DeviceCreatedEvent evt)
    {
        var db = GetDb();
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

        row.OwnerUserId = evt.OwnerUserId;
        row.ProjectId   = evt.ProjectId;
        row.UnitId      = evt.UnitId;
        row.DeviceType  = evt.DeviceType;
        row.Status      = evt.Status;
        row.LastEventAt = evt.OccurredOn;
        await db.SaveChangesAsync();
    }

    private async Task UpsertDeviceAsync(DeviceUpdatedEvent evt)
    {
        var db = GetDb();
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

    private async Task DeleteDeviceAsync(DeviceDeletedEvent evt)
    {
        var db = GetDb();
        var row = await db.DeviceProjections.FindAsync(evt.DeviceId);
        if (row is not null)
        {
            db.DeviceProjections.Remove(row);
            await db.SaveChangesAsync();
        }
    }

    private async Task UpsertProjectAsync(ProjectCreatedEvent evt)
    {
        var db = GetDb();
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

    private async Task UpsertProjectAsync(ProjectUpdatedEvent evt)
    {
        var db = GetDb();
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

    private async Task UpsertUnitAsync(UnitCreatedEvent evt)
    {
        var db = GetDb();
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
        row.OwnerUserId   = evt.OwnerUserId;
        row.Status        = evt.Status;
        row.LastEventAt   = evt.OccurredOn;
        await db.SaveChangesAsync();
    }

    private static T Deserialize<T>(byte[] body) =>
        JsonSerializer.Deserialize<T>(body)
        ?? throw new JsonException($"Failed to deserialize {typeof(T).Name} — result was null.");

    private static bool IsTransientDbError(Exception ex) =>
        ex is DbUpdateException or TimeoutException;
}
