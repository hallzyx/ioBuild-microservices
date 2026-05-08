namespace IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Configuration;

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
}
