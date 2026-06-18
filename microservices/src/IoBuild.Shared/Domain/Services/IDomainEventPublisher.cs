using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Shared.Domain.Services;

/// <summary>
/// Abstraction for publishing a domain event to the message broker.
/// Consumed by the <c>OutboxWorker</c> in each producing service (ADR-8).
///
/// Contract: <c>PublishAsync</c> throws on failure. The caller (OutboxWorker) is
/// responsible for wrapping calls in a Polly resilience pipeline (ADR-2).
/// The publisher itself is a clean transport — it does not own retry or circuit-breaker logic.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes the given domain event to the configured topic exchange.
    /// Throws on any AMQP or connection failure.
    /// </summary>
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}
