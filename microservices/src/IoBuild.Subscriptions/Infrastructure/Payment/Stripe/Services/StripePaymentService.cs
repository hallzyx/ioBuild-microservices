using IoBuild.Shared.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Services;

/// <summary>
/// Service for handling subscription payments through Stripe
/// Based on the monolith's working StripePaymentService implementation
/// </summary>
public class StripePaymentService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        ISubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork,
        StripeSettings stripeSettings,
        ILogger<StripePaymentService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _stripeSettings = stripeSettings;
        _logger = logger;

        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    /// <summary>
    /// Creates a Stripe checkout session for a subscription plan
    /// </summary>
    public async Task<(string SessionId, string CheckoutUrl, long AmountInCents)> CreateCheckoutSessionAsync(
        int builderId, int planId, string successUrl, string cancelUrl)
    {
        // Validate that the plan exists
        var plan = await _planRepository.FindByIdAsync(planId);
        if (plan == null)
            throw new ArgumentException($"Plan with ID {planId} not found");

        // Convert price to cents (Stripe uses smallest currency unit)
        long amountInCents = (long)(plan.Price * 100);

        // Clean success URL: remove any existing query parameters to ensure clean redirection
        var baseSuccessUrl = successUrl.Split('?')[0];
        var fullSuccessUrl = $"{baseSuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}";

        // Clean cancel URL
        var baseCancelUrl = cancelUrl.Split('?')[0];

        // Create Stripe checkout session parameters (one-time payment, like monolith)
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = fullSuccessUrl,
            CancelUrl = baseCancelUrl,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "pen",
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Plan {plan.Name}",
                            Description = plan.Description
                        }
                    }
                }
            ],
            Metadata = new Dictionary<string, string>
            {
                { "builder_id", builderId.ToString() },
                { "plan_id", planId.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return (session.Id, session.Url, amountInCents);
    }

    /// <summary>
    /// Confirms a payment and creates or updates the subscription
    /// </summary>
    public async Task<(string Status, int SubscriptionId, bool IsNewSubscription)> ConfirmPaymentAsync(
        int builderId, string sessionId)
    {
        // Retrieve the Stripe session
        var service = new SessionService();
        var session = await service.GetAsync(sessionId);

        // Validate payment status
        if (session.PaymentStatus != "paid")
        {
            return ("pending", 0, false);
        }

        // Extract metadata
        if (!session.Metadata.TryGetValue("plan_id", out var planIdStr) ||
            !int.TryParse(planIdStr, out var planId))
        {
            throw new InvalidOperationException("Plan ID not found in session metadata");
        }

        // Validate plan exists
        var plan = await _planRepository.FindByIdAsync(planId);
        if (plan == null)
            throw new ArgumentException($"Plan with ID {planId} not found");

        // Check if builder already has an active subscription
        var existingSubscription = await _subscriptionRepository.FindActiveByBuilderIdAsync(builderId);

        bool isNewSubscription = false;
        int subscriptionId;

        if (existingSubscription != null)
        {
            // Update existing subscription
            var newEndDate = existingSubscription.EndDate.HasValue && existingSubscription.EndDate > DateTime.UtcNow
                ? existingSubscription.EndDate.Value.AddMonths(1)
                : DateTime.UtcNow.AddMonths(1);
            existingSubscription.Update(
                planId: planId,
                status: SubscriptionStatus.Active,
                startDate: DateTime.UtcNow,
                endDate: newEndDate);

            _subscriptionRepository.Update(existingSubscription);
            subscriptionId = existingSubscription.Id;
        }
        else
        {
            // Create new subscription
            var newSubscription = new Domain.Model.Aggregates.Subscription(
                builderId: builderId,
                planId: planId,
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddMonths(1));
            newSubscription.Activate();

            await _subscriptionRepository.AddAsync(newSubscription);

            subscriptionId = newSubscription.Id;
            isNewSubscription = true;
        }

        await _unitOfWork.CompleteAsync();

        return ("paid", subscriptionId, isNewSubscription);
    }

    /// <summary>
    /// Lists Stripe Checkout Session receipts for a specific builder, ordered by date descending.
    /// Returns an empty list if a Stripe error occurs.
    /// </summary>
    public async Task<IReadOnlyList<(DateTime Date, long AmountInCents, string Currency, string? ReceiptUrl, string Description, string Status)>> ListReceiptsAsync(int builderId)
    {
        try
        {
            var service = new SessionService();
            var sessions = await service.ListAsync(new SessionListOptions
            {
                Limit = 100,
                Expand = new List<string> { "data.payment_intent.latest_charge" }
            });

            var results = sessions.Data
                .Where(s =>
                    s.Metadata != null &&
                    s.Metadata.TryGetValue("builder_id", out var bid) &&
                    bid == builderId.ToString() &&
                    s.PaymentStatus == "paid")
                .OrderByDescending(s => s.Created)
                .Select(s => (
                    Date: s.Created,
                    AmountInCents: s.AmountTotal ?? 0L,
                    Currency: (s.Currency ?? "pen").ToUpperInvariant(),
                    ReceiptUrl: s.PaymentIntent?.LatestCharge?.ReceiptUrl,
                    Description: "Plan payment",
                    Status: s.PaymentStatus
                ))
                .ToList();

            return results;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while listing receipts for builder {BuilderId}", builderId);
            return Array.Empty<(DateTime, long, string, string?, string, string)>();
        }
    }
}
