namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record CreateCheckoutSessionResource(
    int BuilderId,
    int PlanId,
    string SuccessUrl,
    string CancelUrl
);
