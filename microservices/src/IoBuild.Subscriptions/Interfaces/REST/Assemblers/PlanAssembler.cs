using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;
using IoBuild.Subscriptions.Interfaces.REST.Resources;

namespace IoBuild.Subscriptions.Interfaces.REST.Assemblers;

public static class PlanAssembler
{
    public static PlanResource ToResource(Plan entity)
    {
        return new PlanResource(
            entity.Id,
            entity.Name,
            entity.Price,
            entity.Description,
            entity.Features,
            entity.MaxDevices,
            entity.MaxAdministrators,
            entity.SupportLevel,
            entity.HasAPI,
            entity.HasAnalytics
        );
    }

    public static CreatePlanCommand ToCommand(CreatePlanResource resource)
    {
        return new CreatePlanCommand(
            resource.Name,
            resource.Price,
            resource.Description,
            resource.Features,
            resource.MaxDevices,
            resource.MaxAdministrators,
            resource.SupportLevel,
            resource.HasApi,
            resource.HasAnalytics
        );
    }

    public static UpdatePlanCommand ToCommand(int id, UpdatePlanResource resource)
    {
        return new UpdatePlanCommand(
            id,
            resource.Name,
            resource.Price,
            resource.Description,
            resource.Features,
            resource.MaxDevices,
            resource.MaxAdministrators,
            resource.SupportLevel,
            resource.HasApi,
            resource.HasAnalytics
        );
    }
}
