using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;

namespace IoBuild.Subscriptions.Domain.Repositories.Services;

public interface ISubscriptionCommandService
{
    Task<Subscription> Handle(CreateSubscriptionCommand command);
    Task<Subscription> Handle(UpdateSubscriptionCommand command);
    Task<Subscription> RenewAsync(int builderId, int planId, string successUrl, string cancelUrl);
    Task<Subscription> SubscribeAsync(int builderId, int planId, string successUrl, string cancelUrl);
    Task<Subscription> ChangePlanAsync(int builderId, int newPlanId, string successUrl, string cancelUrl);
    Task<bool> ProcessCompletedCheckoutSessionAsync(string eventId, string sessionId, int builderId, int planId);
    Task<Subscription> CancelAsync(int subscriptionId);
}
