using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;
using IoBuild.Subscriptions.Interfaces.REST.Resources;

namespace IoBuild.Subscriptions.Interfaces.REST.Assemblers;

public static class SubscriptionAssembler
{
    public static SubscriptionResource ToResource(Subscription entity)
    {
        return new SubscriptionResource(
            entity.Id,
            entity.BuilderId,
            entity.PlanId,
            entity.Status.ToString(),
            entity.StartDate,
            entity.EndDate,
            entity.Plan is not null ? PlanAssembler.ToResource(entity.Plan) : null
        );
    }

    public static CreateSubscriptionCommand ToCommand(CreateSubscriptionResource resource)
    {
        return new CreateSubscriptionCommand(
            resource.BuilderId,
            resource.PlanId,
            resource.StartDate,
            resource.EndDate
        );
    }

    public static UpdateSubscriptionCommand ToCommand(int id, UpdateSubscriptionResource resource)
    {
        return new UpdateSubscriptionCommand(
            id,
            resource.BuilderId,
            resource.PlanId,
            resource.StartDate,
            resource.EndDate
        );
    }
}
