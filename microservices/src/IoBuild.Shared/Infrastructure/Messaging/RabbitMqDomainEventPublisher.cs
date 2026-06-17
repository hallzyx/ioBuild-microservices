using System.Text;
using System.Text.Json;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Services;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace IoBuild.Shared.Infrastructure.Messaging;

/// <summary>
/// Publishes domain events to the RabbitMQ topic exchange <c>iobuild.domain.events</c>.
///
/// Design decisions (ADR-1, ADR-2, ADR-8):
/// - Singleton: owns one long-lived IConnection (AMQP connections are expensive).
/// - Creates a fresh channel per publish (channels are cheap; avoids shared-channel concurrency issues).
/// - Publisher confirms enabled (so the OutboxWorker knows the broker actually accepted the message).
/// - Sets AMQP header <c>event-type</c> so the consumer can deserialize to the correct record
///   without a discriminator inside the body (ADR-4).
/// - Uses <c>DeliveryModes.Persistent</c> so messages survive a broker restart (ADR-3).
/// - Does NOT own the Polly circuit breaker. The OutboxWorker wraps calls to this publisher
///   in a ResiliencePipeline (ADR-2). This keeps the publisher cleanly testable.
/// - Throws on any AMQP failure — the worker catches and handles (RetryCount++, stays Pending).
/// </summary>
public sealed class RabbitMqDomainEventPublisher : IDomainEventPublisher, IAsyncDisposable
{
    private const string ExchangeName = "iobuild.domain.events";
    private const string ExchangeType = "topic";

    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqDomainEventPublisher> _logger;
    private bool _exchangeDeclared;

    public RabbitMqDomainEventPublisher(
        IConnection connection,
        ILogger<RabbitMqDomainEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventTypeName = domainEvent.GetType().Name;
        var routingKey = domainEvent.RoutingKey;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));

        // Create a channel with publisher confirms so the OutboxWorker knows the broker accepted.
        var channelOpts = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        await using var channel = await _connection.CreateChannelAsync(channelOpts, cancellationToken);

        // Idempotent exchange declare — safe to call on every publish (broker ignores if args match).
        if (!_exchangeDeclared)
        {
            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType,
                durable: true,
                autoDelete: false,
                arguments: null,
                passive: false,
                noWait: false,
                cancellationToken: cancellationToken);

            _exchangeDeclared = true;
        }

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?> { ["event-type"] = eventTypeName }
        };

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Published {EventType} with EventId={EventId} to exchange={Exchange} routingKey={RoutingKey}",
            eventTypeName, domainEvent.EventId, ExchangeName, routingKey);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.IsOpen)
            await _connection.CloseAsync();
    }
}
