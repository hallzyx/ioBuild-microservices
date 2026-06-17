namespace IoBuild.Devices.Domain.Model.Entities;

/// <summary>
/// Outbox row for the Devices service (ADR-8b).
/// Mirrors <c>IoBuild.Subscriptions.OutboxMessage</c> with one addition: <see cref="EventId"/>
/// (the domain event's stable ID for end-to-end tracing and at-least-once de-duplication).
///
/// Lifecycle: Pending → Processed (on successful publish by OutboxWorker).
/// On publish failure: stays Pending, RetryCount incremented.
/// </summary>
public class OutboxMessage
{
    public int Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }

    /// <summary>
    /// The domain event's <see cref="IoBuild.Shared.Domain.Model.Events.DomainEvent.EventId"/>.
    /// Persisted so the same EventId survives retries for end-to-end tracing (ADR-5).
    /// </summary>
    public Guid EventId { get; set; }

    protected OutboxMessage() { }

    public OutboxMessage(string eventType, string payload)
    {
        EventType = eventType;
        Payload = payload;
    }
}
