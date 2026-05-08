using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Queries;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;

namespace IoBuild.Subscriptions.Application.Services;

public class PlanQueryService : IPlanQueryService
{
    private readonly IPlanRepository _planRepository;

    public PlanQueryService(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query)
    {
        return await _planRepository.ListAsync();
    }

    public async Task<Plan?> Handle(GetPlanByIdQuery query)
    {
        return await _planRepository.FindByIdAsync(query.Id);
    }

    public async Task<Plan?> Handle(GetPlanByNameQuery query)
    {
        return await _planRepository.FindByNameAsync(query.Name);
    }
}
