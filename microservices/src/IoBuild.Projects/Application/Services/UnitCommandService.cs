using System.Text.Json;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Units;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Projects.Application.Services;

public class UnitCommandService : IUnitCommandService
{
    private readonly IUnitRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxMessageRepository _outboxRepository;
    private readonly IProjectRepository _projectRepository;

    public UnitCommandService(
        IUnitRepository repository,
        IUnitOfWork unitOfWork,
        IOutboxMessageRepository outboxRepository,
        IProjectRepository projectRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
        _projectRepository = projectRepository;
    }

    public async Task<int> Handle(CreateUnitCommand command)
    {
        var unit = new Unit(
            command.ProjectId,
            command.UnitNumber,
            command.OwnerId);

        await _repository.AddAsync(unit);

        // Resolve BuilderUserId from the parent Project (Unit has no BuilderUserId directly).
        // If the parent project is not found (defensive guard), BuilderUserId is set to 0
        // and the Analytics read model can resolve it via project_id → project_projection.builder_user_id.
        var parentProject = await _projectRepository.FindByIdAsync(command.ProjectId);
        var builderUserId = parentProject?.BuilderId ?? 0;

        // Build and serialize the domain event for the outbox (ADR-8, REQ-DE-02, DE-S07)
        var evt = new UnitCreatedEvent
        {
            UnitId = unit.Id,
            ProjectId = unit.ProjectId,
            BuilderUserId = builderUserId,
            OwnerUserId = unit.OwnerId,
            Status = "Active"
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(UnitCreatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await _outboxRepository.AddAsync(outboxMessage);

        // Single CompleteAsync covers BOTH the unit row and the outbox row (REQ-DE-02)
        await _unitOfWork.CompleteAsync();
        return unit.Id;
    }
}
