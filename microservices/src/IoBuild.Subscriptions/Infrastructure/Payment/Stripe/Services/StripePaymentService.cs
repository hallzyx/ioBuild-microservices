using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Configuration;
using Stripe;
using Stripe.Checkout;

namespace IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Services;

public class StripePaymentService
{
    private readonly StripeSettings _settings;

    public StripePaymentService(StripeSettings settings)
    {
        _settings = settings;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<(string SessionId, string CheckoutUrl, long AmountInCents)> CreateCheckoutSessionAsync(
        int builderId, int planId, string successUrl, string cancelUrl)
    {
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            Currency = "pen",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "builder_id", builderId.ToString() },
                { "plan_id", planId.ToString() }
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "pen",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"IoBuild Plan #{planId}",
                        },
                        UnitAmount = 0,
                        Recurring = new SessionLineItemPriceDataRecurringOptions
                        {
                            Interval = "month",
                        },
                    },
                    Quantity = 1,
                }
            ],
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return (session.Id, session.Url, session.AmountTotal ?? 0);
    }

    public async Task<(string Status, int SubscriptionId, bool IsNewSubscription)> ConfirmPaymentAsync(
        int builderId, string sessionId)
    {
        var service = new SessionService();
        var session = await service.GetAsync(sessionId);

        var status = session.PaymentStatus switch
        {
            "paid" => "completed",
            "unpaid" => "pending",
            "no_payment_required" => "completed",
            _ => "failed"
        };

        var subscriptionId = int.TryParse(
            session.Metadata?.GetValueOrDefault("plan_id"), out var id) ? id : 0;

        return (status, subscriptionId, session.Status == "complete");
    }
}
