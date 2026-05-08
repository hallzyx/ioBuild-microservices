namespace IoBuild.Subscriptions.Domain.Model.Aggregates;

public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled,
    Pending
}

public class Subscription
{
    public int Id { get; private set; }
    public int BuilderId { get; private set; }
    public int PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Plan Plan { get; private set; } = null!;

    public Subscription() { }

    public Subscription(int builderId, int planId, DateTime startDate, DateTime? endDate)
    {
        BuilderId = builderId;
        PlanId = planId;
        Status = SubscriptionStatus.Pending;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
    }

    public void Expire()
    {
        Status = SubscriptionStatus.Expired;
    }
}
