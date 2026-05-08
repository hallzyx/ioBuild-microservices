namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record PaymentConfirmationResource(
    string Status,
    int SubscriptionId,
    bool IsNewSubscription
);
