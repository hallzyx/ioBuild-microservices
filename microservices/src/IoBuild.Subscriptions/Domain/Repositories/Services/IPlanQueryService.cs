using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Queries;

namespace IoBuild.Subscriptions.Domain.Repositories.Services;

public interface IPlanQueryService
{
    Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query);
    Task<Plan?> Handle(GetPlanByIdQuery query);
    Task<Plan?> Handle(GetPlanByNameQuery query);
}
