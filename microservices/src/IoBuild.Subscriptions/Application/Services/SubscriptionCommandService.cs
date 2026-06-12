using System.Text.Json;
using IoBuild.Shared.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;
using IoBuild.Subscriptions.Domain.Model.Entities;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;
using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Services;

namespace IoBuild.Subscriptions.Application.Services;

public class SubscriptionCommandService : ISubscriptionCommandService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxMessageRepository _outboxRepo;
    private readonly IIdempotencyKeyRepository _idempotencyRepo;
    private readonly IPlanRepository _planRepo;
    private readonly StripePaymentService _stripeService;

    public SubscriptionCommandService(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        IOutboxMessageRepository outboxRepo,
        IIdempotencyKeyRepository idempotencyRepo,
        IPlanRepository planRepo,
        StripePaymentService stripeService)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _outboxRepo = outboxRepo;
        _idempotencyRepo = idempotencyRepo;
        _planRepo = planRepo;
        _stripeService = stripeService;
    }

    public async Task<Subscription> Handle(CreateSubscriptionCommand command)
    {
        var subscription = new Subscription(
            command.BuilderId,
            command.PlanId,
            command.StartDate,
            command.EndDate);

        await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.CompleteAsync();

        return subscription;
    }

    public async Task<Subscription> Handle(UpdateSubscriptionCommand command)
    {
        var subscription = await _subscriptionRepository.FindByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Subscription with id {command.Id} not found.");

        var updated = new Subscription(
            command.BuilderId,
            command.PlanId,
            command.StartDate,
            command.EndDate);

        _subscriptionRepository.Update(updated);
        await _unitOfWork.CompleteAsync();

        return updated;
    }

    public async Task<Subscription> RenewAsync(int builderId, int planId, string successUrl, string cancelUrl)
    {
        // 1. Check idempotency
        var idKey = $"renew_{builderId}_{planId}";
        if (await _idempotencyRepo.ExistsAsync(idKey))
            throw new InvalidOperationException($"Renew already in progress for builder {builderId}, plan {planId}");

        var plan = await _planRepo.FindByIdAsync(planId);
        if (plan is null) throw new KeyNotFoundException($"Plan {planId} not found");

        // 2. Find or create subscription
        var sub = await _subscriptionRepository.FindActiveByBuilderIdAsync(builderId);
        if (sub is null)
        {
            sub = new Subscription(builderId, planId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
            await _subscriptionRepository.AddAsync(sub);
        }
        else
        {
            var newEndDate = sub.EndDate.HasValue && sub.EndDate > DateTime.UtcNow
                ? sub.EndDate.Value.AddMonths(1)
                : DateTime.UtcNow.AddMonths(1);
            sub.Update(endDate: newEndDate);
            _subscriptionRepository.Update(sub);
        }

        // 3. Create Stripe session
        await _stripeService.CreateCheckoutSessionAsync(builderId, planId, successUrl, cancelUrl);

        // 4. Record idempotency key
        await _idempotencyRepo.AddAsync(new IdempotencyKey(idKey));

        await _unitOfWork.CompleteAsync();
        return sub;
    }

    public async Task<bool> ProcessCompletedCheckoutSessionAsync(string eventId, string sessionId, int builderId, int planId)
    {
        // Check idempotency FIRST (before any work)
        if (await _idempotencyRepo.ExistsAsync(eventId))
            return true; // Already processed, silently return

        var sub = await _subscriptionRepository.FindActiveByBuilderIdAsync(builderId);
        if (sub is null)
        {
            sub = new Subscription(builderId, planId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
            sub.Activate();
            await _subscriptionRepository.AddAsync(sub);
        }
        else
        {
            var newEndDate = sub.EndDate.HasValue && sub.EndDate > DateTime.UtcNow
                ? sub.EndDate.Value.AddMonths(1)
                : DateTime.UtcNow.AddMonths(1);
            sub.Update(status: SubscriptionStatus.Active, endDate: newEndDate);
            _subscriptionRepository.Update(sub);
        }

        // Insert outbox message
        var outbox = new OutboxMessage("subscription.activated",
            JsonSerializer.Serialize(new { builderId, planId, subscriptionId = sub.Id, newEndDate = sub.EndDate }));
        await _outboxRepo.AddAsync(outbox);

        // Record idempotency key
        await _idempotencyRepo.AddAsync(new IdempotencyKey(eventId));

        // Commit all in one ACID transaction
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
