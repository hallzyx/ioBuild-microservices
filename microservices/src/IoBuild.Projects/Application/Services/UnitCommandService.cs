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

        // ADR-A — Two-phase commit to fix the unit.Id == 0 bug.
        //
        // Phase 1: persist the Unit aggregate so the database assigns its real
        // identity (auto-increment PK). On MySQL/Pomelo the Id property is 0
        // until SaveChanges executes the INSERT and returns the generated key.
        // Building UnitCreatedEvent before this commit would ship UnitId = 0,
        // corrupting the Analytics UnitProjection PK (the original bug).
        //
        // Phase 2: with the real Id in hand, build UnitCreatedEvent and persist
        // the outbox row. A second CompleteAsync commits only the outbox row.
        //
        // Trade-off: if the process crashes between the two commits, the Unit
        // exists without its outbox event. The existing idempotent OutboxBackfill
        // (Projects/Infrastructure/Persistence/EFC/Configuration/Seed/) re-emits
        // missing events on next startup, so at-least-once delivery is preserved.
        // This is consistent with the Devices and IAM patterns (ADR-A).

        await _unitOfWork.CompleteAsync(); // Phase 1 — unit.Id is now real

        // Resolve BuilderUserId from the parent Project (Unit has no BuilderUserId directly).
        // If the parent project is not found (defensive guard), BuilderUserId defaults to 0
        // and Analytics can resolve it via project_id → project_projection.builder_user_id.
        var parentProject = await _projectRepository.FindByIdAsync(command.ProjectId);
        var builderUserId = parentProject?.BuilderId ?? 0;

        // Build the domain event AFTER Phase 1 so UnitId carries the real identity (ADR-A).
        var evt = new UnitCreatedEvent
        {
            UnitId = unit.Id,          // real DB-assigned id
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

        await _unitOfWork.CompleteAsync(); // Phase 2 — outbox row committed (REQ-DE-02)

        return unit.Id;
    }

    public async Task Handle(AssignUnitOwnerEmailCommand command)
    {
        var unit = await _repository.FindByIdAsync(command.UnitId);
        if (unit == null)
            throw new KeyNotFoundException($"Unit {command.UnitId} not found.");

        unit.AssignOwnerEmail(command.OwnerEmail);
        _repository.Update(unit);

        // Recompute Project totals: OccupiedUnits = count of units with OwnerEmail != null
        var project = await _projectRepository.FindByIdAsync(unit.ProjectId);
        if (project != null)
        {
            var allUnits = await _repository.FindByProjectIdAsync(unit.ProjectId);
            var totalUnits = allUnits.Count();
            var occupiedUnits = allUnits.Count(u => u.OwnerEmail != null);
            project.Update(
                project.Name,
                project.Description,
                project.Location,
                totalUnits,
                occupiedUnits,
                project.Status,
                project.ImageUrl);
            _projectRepository.Update(project);
        }

        await _unitOfWork.CompleteAsync();
    }
}
