namespace IoBuild.Projects.Domain.Model.Entities;

/// <summary>
/// Outbox row for the Projects service (ADR-8b).
/// Mirrors IoBuild.Devices.Domain.Model.Entities.OutboxMessage exactly.
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
    /// The domain event's stable EventId for end-to-end tracing and at-least-once de-duplication (ADR-5).
    /// </summary>
    public Guid EventId { get; set; }

    protected OutboxMessage() { }

    public OutboxMessage(string eventType, string payload)
    {
        EventType = eventType;
        Payload = payload;
    }
}
