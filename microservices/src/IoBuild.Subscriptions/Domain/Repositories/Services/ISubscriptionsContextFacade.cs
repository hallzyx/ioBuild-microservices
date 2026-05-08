using IoBuild.Subscriptions.Domain.Model.Aggregates;

namespace IoBuild.Subscriptions.Domain.Repositories.Services;

public interface ISubscriptionsContextFacade
{
    Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId);
    Task<Plan?> GetPlanByIdAsync(int planId);
    Task<IEnumerable<Subscription>> GetSubscriptionsByBuilderAsync(int builderId);
    Task<bool> HasActiveSubscriptionAsync(int builderId);
}
