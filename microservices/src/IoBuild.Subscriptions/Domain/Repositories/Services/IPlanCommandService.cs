using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;

namespace IoBuild.Subscriptions.Domain.Repositories.Services;

public interface IPlanCommandService
{
    Task<Plan> Handle(CreatePlanCommand command);
    Task<Plan> Handle(UpdatePlanCommand command);
}
