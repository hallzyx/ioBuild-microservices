using IoBuild.Shared.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Commands;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;

namespace IoBuild.Subscriptions.Application.Services;

public class PlanCommandService : IPlanCommandService
{
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlanCommandService(
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Plan> Handle(CreatePlanCommand command)
    {
        var plan = new Plan(
            command.Name,
            command.Price,
            command.Description,
            command.Features,
            command.MaxDevices,
            command.MaxAdministrators,
            command.SupportLevel,
            command.HasApi,
            command.HasAnalytics);

        await _planRepository.AddAsync(plan);
        await _unitOfWork.CompleteAsync();

        return plan;
    }

    public async Task<Plan> Handle(UpdatePlanCommand command)
    {
        var plan = await _planRepository.FindByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Plan with id {command.Id} not found.");

        plan.Update(
            command.Name,
            command.Price,
            command.Description,
            command.Features,
            command.MaxDevices,
            command.MaxAdministrators,
            command.SupportLevel,
            command.HasApi,
            command.HasAnalytics);

        _planRepository.Update(plan);
        await _unitOfWork.CompleteAsync();

        return plan;
    }
}
