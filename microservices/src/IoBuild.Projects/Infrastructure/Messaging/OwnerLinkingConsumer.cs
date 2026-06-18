using System.Text;
using System.Text.Json;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Infrastructure.Persistence;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IoBuild.Projects.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes IAM user-registration events and links
/// project units to their owners via email matching (§3.1–3.2, REQ-OL-02..OL-06).
///
/// Topology (ADR-3):
///   Exchange : iobuild.domain.events (topic, durable)
///   Queue    : projects.owner-linking  (durable)
///   Binding  : iam.user.#
///
/// On UserRegisteredEvent where Role == "Owner":
///   1. Upsert a <c>registered_owner</c> row (email PK, lower-cased — ADR-D).
///   2. Query Units where OwnerEmail == email AND OwnerId IS NULL.
///   3. For each match: Unit.LinkOwner(userId) + write UnitOwnerMatchedEvent outbox row.
///   4. Single CompleteAsync (atomic outbox write).
///
/// Idempotency (REQ-OL-06):
///   Units with OwnerId already set are skipped. Registered owner row uses
///   UpdateIfNewer LWW guard.
///
/// Ack semantics (ADR-5):
///   Success          → BasicAck
///   Transient DB err → BasicNack(requeue:true)
///   Poison message   → log + BasicNack(requeue:false)
///
/// DI scope: singleton BackgroundService; fresh DI scope opened per message (production).
/// Test path uses the internal constructor with a direct AppDbContext — calls ApplyEventAsync directly.
/// </summary>
public class OwnerLinkingConsumer : BackgroundService
{
    private const string ExchangeName = "iobuild.domain.events";
    private const string QueueName    = "projects.owner-linking";
    private const string BindingKey   = "iam.user.#";

    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly AppDbContext?         _directDb;   // test path only
    private readonly ILogger<OwnerLinkingConsumer> _logger;
    private readonly string? _connectionString;

    /// <summary>Production constructor — resolves AppDbContext from DI scope per message.</summary>
    public OwnerLinkingConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OwnerLinkingConsumer> logger)
    {
        _scopeFactory     = scopeFactory;
        _logger           = logger;
        _connectionString = configuration["RabbitMq:ConnectionString"];
    }

    /// <summary>
    /// Test constructor — uses a directly-injected AppDbContext (no broker connection).
    /// Mirrors AnalyticsEventConsumer.internal ctor pattern (§9.3).
    /// </summary>
    internal OwnerLinkingConsumer(
        AppDbContext db,
        ILogger<OwnerLinkingConsumer> logger)
    {
        _directDb = db;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning(
                "OwnerLinkingConsumer: RabbitMq:ConnectionString is not configured — consumer is disabled.");
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

        IConnection? connection = null;
        IChannel?    channel    = null;

        try
        {
            connection = await factory.CreateConnectionAsync(stoppingToken);
            channel    = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Declare exchange (idempotent — matches other consumers)
            await channel.ExchangeDeclareAsync(
                exchange:   ExchangeName,
                type:       ExchangeType.Topic,
                durable:    true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            // Declare queue (durable, exclusive to this consumer's role)
            await channel.QueueDeclareAsync(
                queue:      QueueName,
                durable:    true,
                exclusive:  false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            // Bind to IAM user routing-key pattern
            await channel.QueueBindAsync(QueueName, ExchangeName, BindingKey,
                cancellationToken: stoppingToken);

            // Manual ack — one message at a time for safety during owner-linking writes
            await channel.BasicQosAsync(
                prefetchSize:  0,
                prefetchCount: 1,
                global:        false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                await HandleDeliveryAsync(channel, ea, stoppingToken);
            };

            await channel.BasicConsumeAsync(
                queue:    QueueName,
                autoAck:  false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "OwnerLinkingConsumer started. Listening on queue '{Queue}' (binding: {Binding})",
                QueueName, BindingKey);

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OwnerLinkingConsumer: fatal error in consumer loop");
        }
        finally
        {
            if (channel    is not null) await channel.CloseAsync();
            if (connection is not null) await connection.CloseAsync();
        }
    }

    private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken ct)
    {
        ulong   deliveryTag = ea.DeliveryTag;
        string? eventType   = null;

        try
        {
            if (ea.BasicProperties.Headers is null
                || !ea.BasicProperties.Headers.TryGetValue("event-type", out var rawType))
            {
                _logger.LogWarning(
                    "OwnerLinkingConsumer: missing event-type header. Nacking without requeue.");
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            eventType = rawType is byte[] bytes
                ? Encoding.UTF8.GetString(bytes)
                : rawType?.ToString();

            var body = ea.Body.ToArray();

            if (_scopeFactory is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ApplyEventByTypeAsync(eventType!, body, db, ct);
            }
            else
            {
                await ApplyEventByTypeAsync(eventType!, body, _directDb!, ct);
            }

            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (Exception ex) when (IsTransientDbError(ex))
        {
            _logger.LogWarning(ex,
                "OwnerLinkingConsumer: transient DB error for EventType={EventType}. Nacking with requeue.",
                eventType);
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OwnerLinkingConsumer: poison message for EventType={EventType}. Nacking without requeue.",
                eventType);
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task ApplyEventByTypeAsync(string eventType, byte[] body, AppDbContext db, CancellationToken ct)
    {
        switch (eventType)
        {
            case nameof(UserRegisteredEvent):
                await ApplyUserRegisteredAsync(Deserialize<UserRegisteredEvent>(body), db);
                break;
            default:
                _logger.LogWarning(
                    "OwnerLinkingConsumer: unknown EventType={EventType}. Nacking without requeue.",
                    eventType);
                throw new InvalidOperationException($"Unknown event type: {eventType}");
        }
    }

    // ── Public for unit tests (mirrors AnalyticsEventConsumer.ApplyEventAsync) ──

    /// <summary>
    /// Applies a domain event directly against a DbContext.
    /// Tests using the internal constructor call this with a pre-wired AppDbContext.
    /// </summary>
    public async Task ApplyEventAsync(DomainEvent evt)
    {
        if (_directDb is not null)
        {
            await ApplyEventWithDb(evt, _directDb);
            return;
        }

        if (_scopeFactory is not null)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await ApplyEventWithDb(evt, db);
            return;
        }

        throw new InvalidOperationException(
            "OwnerLinkingConsumer: neither _directDb nor _scopeFactory is set.");
    }

    private Task ApplyEventWithDb(DomainEvent evt, AppDbContext db) => evt switch
    {
        UserRegisteredEvent e => ApplyUserRegisteredAsync(e, db),
        _ => throw new InvalidOperationException($"Unsupported event type: {evt.GetType().Name}")
    };

    // ── Core business logic ──

    private async Task ApplyUserRegisteredAsync(UserRegisteredEvent evt, AppDbContext db)
    {
        // Non-owner roles: ack immediately, no mutation (REQ-OL-02)
        if (!string.Equals(evt.Role, "Owner", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug(
                "OwnerLinkingConsumer: skipping non-Owner role '{Role}' for Email={Email}",
                evt.Role, evt.Email);
            return;
        }

        var normalizedEmail = evt.Email.ToLowerInvariant();

        // 1. Upsert registered_owner mirror (async registration-first path, §3.3)
        var existing = await db.RegisteredOwners.FindAsync(normalizedEmail);
        if (existing is null)
        {
            var mirror = new RegisteredOwner(normalizedEmail, evt.UserId, evt.OccurredOn);
            db.RegisteredOwners.Add(mirror);
        }
        else
        {
            existing.UpdateIfNewer(evt.UserId, evt.OccurredOn);
        }

        // 2. Find all units with matching OwnerEmail and no OwnerId (unit-first path, REQ-OL-04)
        var unlinkedUnits = await db.Units
            .Where(u => u.OwnerEmail == normalizedEmail && u.OwnerId == null)
            .ToListAsync();

        // 3. Link each unit and write one outbox row per match
        foreach (var unit in unlinkedUnits)
        {
            unit.LinkOwner(evt.UserId);

            var matchedEvent = new UnitOwnerMatchedEvent
            {
                UnitId      = unit.Id,
                ProjectId   = unit.ProjectId,
                OwnerUserId = evt.UserId,
                OwnerEmail  = normalizedEmail,
            };

            var outboxRow = new OutboxMessage(
                nameof(UnitOwnerMatchedEvent),
                JsonSerializer.Serialize(matchedEvent))
            {
                EventId = matchedEvent.EventId,
            };

            await db.OutboxMessages.AddAsync(outboxRow);

            _logger.LogInformation(
                "OwnerLinkingConsumer: linked UnitId={UnitId} to OwnerId={OwnerId} via email match.",
                unit.Id, evt.UserId);
        }

        // 4. Single CompleteAsync — persists mirror row + unit updates + outbox rows atomically
        await db.CompleteAsync();
    }

    private static T Deserialize<T>(byte[] body) =>
        JsonSerializer.Deserialize<T>(body)
        ?? throw new JsonException($"Failed to deserialize {typeof(T).Name} — result was null.");

    private static bool IsTransientDbError(Exception ex) =>
        ex is DbUpdateException or TimeoutException;
}
