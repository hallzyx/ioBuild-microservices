namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>
/// Base record for all domain events in the IoBuild platform.
/// Implements <see cref="IEvent"/> and provides a unique <see cref="EventId"/> for
/// idempotency tracking and end-to-end tracing (ADR-4, ADR-5).
/// </summary>
public abstract record DomainEvent : IEvent
{
    /// <summary>Stable unique identifier for the event occurrence. Used by consumers for de-duplication.</summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>UTC timestamp set at event creation.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Self-describing routing key (format: <c>domain.aggregate.action</c>).
    /// Used by the OutboxWorker to route the message to the correct exchange binding.
    /// </summary>
    public abstract string RoutingKey { get; }
}
