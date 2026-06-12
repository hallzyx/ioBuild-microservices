using IoBuild.Subscriptions.Domain.Repositories.Services;
using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Configuration;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace IoBuild.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly StripeSettings _stripeSettings;
    private readonly ISubscriptionCommandService _commandService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        StripeSettings stripeSettings,
        ISubscriptionCommandService commandService,
        ILogger<WebhookController> logger)
    {
        _stripeSettings = stripeSettings;
        _commandService = commandService;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _stripeSettings.WebhookSecret);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session?.PaymentStatus != "paid")
                {
                    _logger.LogInformation("Checkout session {SessionId} has status {PaymentStatus}, skipping activation",
                        session?.Id, session?.PaymentStatus);
                    return Ok(new { received = true, status = "unpaid" });
                }

                var builderId = int.Parse(session.Metadata["builder_id"]);
                var planId = int.Parse(session.Metadata["plan_id"]);

                _logger.LogInformation("Processing completed checkout session {SessionId} for builder {BuilderId}, plan {PlanId}",
                    session.Id, builderId, planId);

                await _commandService.ProcessCompletedCheckoutSessionAsync(
                    stripeEvent.Id, session.Id, builderId, planId);
            }

            return Ok(new { received = true, eventId = stripeEvent.Id });
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Invalid Stripe webhook signature: {Message}", ex.Message);
            return Unauthorized(new { error = "Invalid webhook signature" });
        }
    }
}
