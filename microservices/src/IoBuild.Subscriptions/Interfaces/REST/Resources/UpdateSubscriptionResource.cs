namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record UpdateSubscriptionResource(
    int BuilderId,
    int PlanId,
    DateTime StartDate,
    DateTime? EndDate
);
