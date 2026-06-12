namespace IoBuild.Subscriptions.Domain.Model.Commands;

public record RenewSubscriptionCommand(int BuilderId, int PlanId, string SuccessUrl, string CancelUrl);
