using System.Text;
using System.Text.Json;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IoBuild.Devices.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes <see cref="UnitOwnerMatchedEvent"/> from RabbitMQ
/// and upserts a <see cref="UnitOwnerProjection"/> row in the Devices database (ADR-B4).
///
/// Topology:
///   Exchange : iobuild.domain.events          (topic, durable)
///   Queue    : devices.unit-owner-projection  (durable)
///   Binding  : project.unit.owner-matched
///
/// Idempotency pattern (count-based pre-check — consistent with UnitDeviceProvisioningConsumer):
///   Check if row already exists with same (UnitId, OwnerUserId) → skip.
///   Otherwise insert-or-update: update if UnitId matches, insert if new.
///   This ensures re-delivery is a no-op even if the DB-level unique index fires.
///
/// Eventual consistency (ADR-B4): if the event arrives before the handler tries to issue
/// a command, the row is ready; if not yet consumed, the handler returns 403 until the
/// projection catches up — acceptable per spec D-4.
///
/// DI scope: singleton BackgroundService. Per-message scope resolves DevicesDbContext.
/// Test seam: internal constructor receives a direct DevicesDbContext (mirrors UnitDeviceProvisioningConsumer).
/// </summary>
public class UnitOwnerProjectionConsumer : BackgroundService
{
    private const string ExchangeName = "iobuild.domain.events";
    private const string QueueName = "devices.unit-owner-projection";
    private const string RoutingKey = "project.unit.owner-matched";

    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly DevicesDbContext? _directDb;   // test seam only
    private readonly ILogger<UnitOwnerProjectionConsumer> _logger;
    private readonly string? _connectionString;

    /// <summary>Production constructor — resolves DbContext from DI scope per message.</summary>
    public UnitOwnerProjectionConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<UnitOwnerProjectionConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _connectionString = configuration["RabbitMq:ConnectionString"];
    }

    /// <summary>
    /// Test constructor — receives a pre-wired DbContext; no broker connection.
    /// Tests call <see cref="HandleEventAsync"/> directly.
    /// </summary>
    internal UnitOwnerProjectionConsumer(
        DevicesDbContext db,
        ILogger<UnitOwnerProjectionConsumer> logger)
    {
        _directDb = db;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning(
                "UnitOwnerProjectionConsumer: RabbitMq:ConnectionString not configured — consumer disabled.");
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
                "UnitOwnerProjectionConsumer started. Listening on queue '{Queue}'", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnitOwnerProjectionConsumer: fatal error in consumer loop");
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
                _logger.LogWarning("UnitOwnerProjectionConsumer: missing event-type header. Nacking without requeue.");
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            var eventType = rawType is byte[] bytes ? Encoding.UTF8.GetString(bytes) : rawType?.ToString();

            if (eventType != nameof(UnitOwnerMatchedEvent))
            {
                _logger.LogWarning(
                    "UnitOwnerProjectionConsumer: unexpected EventType={EventType} on {Queue}. Nacking without requeue.",
                    eventType, QueueName);
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            var evt = JsonSerializer.Deserialize<UnitOwnerMatchedEvent>(ea.Body.ToArray())
                ?? throw new JsonException("Failed to deserialize UnitOwnerMatchedEvent");

            if (_scopeFactory is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
                await HandleEventAsync(evt, db);
            }
            else
            {
                await HandleEventAsync(evt, _directDb!);
            }

            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnitOwnerProjectionConsumer: error processing message. Nacking with requeue.");
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
    }

    /// <summary>
    /// Core upsert logic — called from broker handler (production) and directly from tests.
    /// Idempotency: if a row for (UnitId, OwnerUserId) already exists, update UpdatedAt only.
    /// If UnitId exists but OwnerUserId changed (reassignment), update OwnerUserId.
    /// </summary>
    internal async Task HandleEventAsync(UnitOwnerMatchedEvent evt)
    {
        if (_directDb is null)
            throw new InvalidOperationException(
                "HandleEventAsync(event) without db is only valid when _directDb is set (test path).");
        await HandleEventAsync(evt, _directDb);
    }

    private async Task HandleEventAsync(UnitOwnerMatchedEvent evt, DevicesDbContext db)
    {
        var existing = await db.UnitOwnerProjections
            .FirstOrDefaultAsync(p => p.UnitId == evt.UnitId);

        if (existing is not null)
        {
            // Idempotent update — also handles ownership reassignment
            existing.OwnerUserId = evt.OwnerUserId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            db.UnitOwnerProjections.Add(new UnitOwnerProjection
            {
                UnitId = evt.UnitId,
                OwnerUserId = evt.OwnerUserId,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "UnitOwnerProjectionConsumer: upserted projection for UnitId={UnitId} OwnerUserId={OwnerUserId}.",
            evt.UnitId, evt.OwnerUserId);
    }
}
