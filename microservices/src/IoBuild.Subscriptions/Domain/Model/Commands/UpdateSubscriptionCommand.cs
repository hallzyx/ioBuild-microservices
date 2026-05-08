namespace IoBuild.Subscriptions.Domain.Model.Commands;

public record UpdateSubscriptionCommand(
    int Id,
    int BuilderId,
    int PlanId,
    DateTime StartDate,
    DateTime? EndDate
);
