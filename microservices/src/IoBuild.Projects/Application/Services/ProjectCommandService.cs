using System.Text.Json;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Projects.Application.Services;

public class ProjectCommandService : IProjectCommandService
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxMessageRepository _outboxRepository;

    public ProjectCommandService(
        IProjectRepository repository,
        IUnitOfWork unitOfWork,
        IOutboxMessageRepository outboxRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
    }

    public async Task<int> Handle(CreateProjectCommand command)
    {
        var project = new Project(
            command.Name,
            command.Description,
            command.Location,
            command.TotalUnits,
            command.BuilderId,
            command.ImageUrl);

        await _repository.AddAsync(project);

        // Phase 1: persist the project so the DB assigns the real identity (ADR-A two-phase commit,
        // mirroring ProjectStructureCommandService). Building the event before this commit would
        // capture project.Id == 0 — the bug that broke the builder analytics rollup (GitHub #3).
        await _unitOfWork.CompleteAsync();

        // Phase 2: build the event with the real project.Id, then persist the outbox row.
        // BuilderUserId maps directly from Project.BuilderId.
        var evt = new ProjectCreatedEvent
        {
            ProjectId = project.Id,
            BuilderUserId = project.BuilderId,
            Name = project.Name,
            Status = project.Status.ToString()
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(ProjectCreatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await _outboxRepository.AddAsync(outboxMessage);
        await _unitOfWork.CompleteAsync(); // Phase 2 commit — outbox row with the real ProjectId
        return project.Id;
    }

    public async Task Handle(UpdateProjectCommand command)
    {
        var project = await _repository.FindByIdAsync(command.Id);
        if (project == null)
            throw new KeyNotFoundException($"Project with id {command.Id} not found.");

        // TotalUnits and OccupiedUnits are DERIVED counters maintained by the
        // structure-definition and owner-assignment flows (DefineStructure sets
        // TotalUnits; UnitCommandService recomputes both from actual units). They must
        // never be set from a project edit — preserve the existing values so a stale
        // form payload cannot clobber them (which read as "the owner vanished").
        project.Update(
            command.Name,
            command.Description,
            command.Location,
            project.TotalUnits,
            project.OccupiedUnits,
            command.Status,
            command.ImageUrl);

        _repository.Update(project);

        var evt = new ProjectUpdatedEvent
        {
            ProjectId = project.Id,
            BuilderUserId = project.BuilderId,
            Name = project.Name,
            Status = project.Status.ToString()
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(ProjectUpdatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await _outboxRepository.AddAsync(outboxMessage);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(DeleteProjectCommand command)
    {
        var project = await _repository.FindByIdAsync(command.Id);
        if (project == null)
            throw new KeyNotFoundException($"Project with id {command.Id} not found.");

        _repository.Remove(project);
        await _unitOfWork.CompleteAsync();
    }
}
