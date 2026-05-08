using IoBuild.Shared.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Model.Aggregates;

namespace IoBuild.Subscriptions.Domain.Repositories;

public interface IPlanRepository : IBaseRepository<Plan>
{
    Task<Plan?> FindByNameAsync(string name);
}
