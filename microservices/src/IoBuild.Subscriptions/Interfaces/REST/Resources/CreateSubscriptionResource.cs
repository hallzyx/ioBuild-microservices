namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record CreateSubscriptionResource(
    int BuilderId,
    int PlanId,
    DateTime StartDate,
    DateTime? EndDate
);
