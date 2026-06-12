namespace IoBuild.Subscriptions.Domain.Model.Entities;

public class OutboxMessage
{
    public int Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }

    protected OutboxMessage() { }

    public OutboxMessage(string eventType, string payload)
    {
        EventType = eventType;
        Payload = payload;
    }
}
