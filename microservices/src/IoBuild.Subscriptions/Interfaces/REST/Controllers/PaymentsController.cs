using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Services;
using IoBuild.Subscriptions.Interfaces.REST.Assemblers;
using IoBuild.Subscriptions.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly StripePaymentService _stripePaymentService;

    public PaymentsController(StripePaymentService stripePaymentService)
    {
        _stripePaymentService = stripePaymentService;
    }

    [HttpPost("create-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionResource resource)
    {
        var (sessionId, checkoutUrl, amountInCents) =
            await _stripePaymentService.CreateCheckoutSessionAsync(
                resource.BuilderId,
                resource.PlanId,
                resource.SuccessUrl,
                resource.CancelUrl);

        var response = PaymentAssembler.ToCheckoutSessionResource(
            sessionId, checkoutUrl, amountInCents);

        return Ok(response);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentResource resource)
    {
        var (status, subscriptionId, isNewSubscription) =
            await _stripePaymentService.ConfirmPaymentAsync(
                resource.BuilderId, resource.SessionId);

        var response = PaymentAssembler.ToConfirmationResource(
            status, subscriptionId, isNewSubscription);

        return Ok(response);
    }
}
