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

        // Build and serialize the domain event for the outbox (ADR-8, REQ-DE-02)
        // Note: project.Id is 0 until SaveChanges assigns it (identity column).
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

        // Single CompleteAsync covers BOTH the project row and the outbox row (REQ-DE-02)
        await _unitOfWork.CompleteAsync();
        return project.Id;
    }

    public async Task Handle(UpdateProjectCommand command)
    {
        var project = await _repository.FindByIdAsync(command.Id);
        if (project == null)
            throw new KeyNotFoundException($"Project with id {command.Id} not found.");

        project.Update(
            command.Name,
            command.Description,
            command.Location,
            command.TotalUnits,
            command.OccupiedUnits,
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
