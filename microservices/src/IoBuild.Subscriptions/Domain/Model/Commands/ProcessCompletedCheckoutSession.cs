namespace IoBuild.Subscriptions.Domain.Model.Commands;

public record ProcessCompletedCheckoutSession(string EventId, string SessionId, int BuilderId, int PlanId);
