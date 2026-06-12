namespace IoBuild.Subscriptions.Domain.Model.Entities;

public class IdempotencyKey
{
    public string Key { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; } = DateTime.UtcNow.AddHours(24);

    protected IdempotencyKey() { }

    public IdempotencyKey(string key)
    {
        Key = key;
    }
}
