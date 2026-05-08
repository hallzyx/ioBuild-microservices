namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record CheckoutSessionResponseResource(
    string SessionId,
    string CheckoutUrl,
    long AmountInCents
);
