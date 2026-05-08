using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Queries;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;

namespace IoBuild.Subscriptions.Application.Services;

public class SubscriptionQueryService : ISubscriptionQueryService
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public SubscriptionQueryService(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<IEnumerable<Subscription>> Handle(GetAllSubscriptionsQuery query)
    {
        return await _subscriptionRepository.ListAsync();
    }

    public async Task<Subscription?> Handle(GetSubscriptionByIdQuery query)
    {
        return await _subscriptionRepository.FindByIdAsync(query.Id);
    }
}
