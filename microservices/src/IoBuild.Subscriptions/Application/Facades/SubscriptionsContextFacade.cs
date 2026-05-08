using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;

namespace IoBuild.Subscriptions.Application.Facades;

public class SubscriptionsContextFacade : ISubscriptionsContextFacade
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;

    public SubscriptionsContextFacade(
        ISubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
    }

    public async Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId)
    {
        return await _subscriptionRepository.FindByIdAsync(subscriptionId);
    }

    public async Task<Plan?> GetPlanByIdAsync(int planId)
    {
        return await _planRepository.FindByIdAsync(planId);
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByBuilderAsync(int builderId)
    {
        return await _subscriptionRepository.FindByBuilderIdAsync(builderId);
    }

    public async Task<bool> HasActiveSubscriptionAsync(int builderId)
    {
        var subscriptions = await _subscriptionRepository.FindByBuilderIdAsync(builderId);
        return subscriptions.Any(s => s.Status == SubscriptionStatus.Active);
    }
}
