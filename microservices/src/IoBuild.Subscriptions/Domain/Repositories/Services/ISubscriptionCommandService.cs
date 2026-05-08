using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;

namespace IoBuild.Subscriptions.Domain.Repositories.Services;

public interface ISubscriptionCommandService
{
    Task<Subscription> Handle(CreateSubscriptionCommand command);
    Task<Subscription> Handle(UpdateSubscriptionCommand command);
}
