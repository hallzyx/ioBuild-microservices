using IoBuild.Subscriptions.Interfaces.REST.Resources;

namespace IoBuild.Subscriptions.Interfaces.REST.Assemblers;

public static class PaymentAssembler
{
    public static CheckoutSessionResponseResource ToCheckoutSessionResource(
        string sessionId, string checkoutUrl, long amountInCents)
    {
        return new CheckoutSessionResponseResource(sessionId, checkoutUrl, amountInCents);
    }

    public static PaymentConfirmationResource ToConfirmationResource(
        string status, int subscriptionId, bool isNewSubscription)
    {
        return new PaymentConfirmationResource(status, subscriptionId, isNewSubscription);
    }
}
