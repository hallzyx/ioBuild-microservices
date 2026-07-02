using IoBuild.Subscriptions.Domain.Model.Queries;
using IoBuild.Subscriptions.Domain.Repositories.Services;
using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Services;
using IoBuild.Subscriptions.Interfaces.REST.Assemblers;
using IoBuild.Subscriptions.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/subscriptions/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly StripePaymentService _stripePaymentService;
    private readonly ISubscriptionQueryService _subscriptionQueryService;

    public PaymentsController(
        StripePaymentService stripePaymentService,
        ISubscriptionQueryService subscriptionQueryService)
    {
        _stripePaymentService = stripePaymentService;
        _subscriptionQueryService = subscriptionQueryService;
    }

    [HttpPost("sessions")]
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

        return StatusCode(201, response);
    }

    [HttpPost("sessions/confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentResource resource)
    {
        var (status, subscriptionId, isNewSubscription) =
            await _stripePaymentService.ConfirmPaymentAsync(
                resource.BuilderId, resource.SessionId);

        var response = PaymentAssembler.ToConfirmationResource(
            status, subscriptionId, isNewSubscription);

        return Ok(response);
    }

    // builderId is a query parameter (not a route segment) on purpose: the global
    // kebab-case route convention rewrites camelCase route tokens (e.g. {builderId}
    // -> {builder-id}), which breaks binding to the camelCase action parameter.
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] int builderId)
    {
        // Filter to invoices created on or after the current subscription start date so
        // prior-cycle Stripe sessions (from previous demo runs that reuse builder_id=1)
        // don't bleed through.
        var subscription = await _subscriptionQueryService.GetCurrentSubscriptionAsync(builderId);
        // Subtract 10 minutes from StartDate to cover the gap between Stripe session
        // creation (before checkout) and subscription record creation (after confirm).
        var since = subscription?.StartDate.AddMinutes(-10);
        var receipts = await _stripePaymentService.ListReceiptsAsync(builderId, since);

        var list = receipts.Select(r => new InvoiceResource(
            Date: r.Date.ToString("o"),
            Amount: r.AmountInCents / 100m,
            Currency: r.Currency,
            ReceiptUrl: r.ReceiptUrl,
            Description: r.Description,
            Status: r.Status
        )).ToList();

        return Ok(list);
    }
}
