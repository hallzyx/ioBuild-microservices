namespace IoBuild.Subscriptions.Domain.Model.Commands;

public record CreateSubscriptionCommand(
    int BuilderId,
    int PlanId,
    DateTime StartDate,
    DateTime? EndDate
);
