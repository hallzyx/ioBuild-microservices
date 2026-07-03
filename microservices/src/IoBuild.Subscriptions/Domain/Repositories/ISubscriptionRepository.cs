using IoBuild.Shared.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Model.Aggregates;

namespace IoBuild.Subscriptions.Domain.Repositories;

public interface ISubscriptionRepository : IBaseRepository<Subscription>
{
    Task<IEnumerable<Subscription>> FindByBuilderIdAsync(int builderId);
    Task<Subscription?> FindByPlanIdAsync(int planId);
    Task<Subscription?> FindActiveByBuilderIdAsync(int builderId);
    Task<Subscription?> FindMostRecentByBuilderIdAsync(int builderId);
}
