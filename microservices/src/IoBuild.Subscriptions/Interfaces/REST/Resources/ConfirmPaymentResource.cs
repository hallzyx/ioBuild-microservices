namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record ConfirmPaymentResource(
    int BuilderId,
    string SessionId
);
