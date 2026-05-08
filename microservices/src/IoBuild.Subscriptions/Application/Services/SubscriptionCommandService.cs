using IoBuild.Shared.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;

namespace IoBuild.Subscriptions.Application.Services;

public class SubscriptionCommandService : ISubscriptionCommandService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionCommandService(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Subscription> Handle(CreateSubscriptionCommand command)
    {
        var subscription = new Subscription(
            command.BuilderId,
            command.PlanId,
            command.StartDate,
            command.EndDate);

        await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.CompleteAsync();

        return subscription;
    }

    public async Task<Subscription> Handle(UpdateSubscriptionCommand command)
    {
        var subscription = await _subscriptionRepository.FindByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Subscription with id {command.Id} not found.");

        var updated = new Subscription(
            command.BuilderId,
            command.PlanId,
            command.StartDate,
            command.EndDate);

        _subscriptionRepository.Update(updated);
        await _unitOfWork.CompleteAsync();

        return updated;
    }
}
