namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record SubscriptionResource(
    int Id,
    int BuilderId,
    int PlanId,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    PlanResource? Plan
);
