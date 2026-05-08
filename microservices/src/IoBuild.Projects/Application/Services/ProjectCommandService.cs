using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Projects.Application.Services;

public class ProjectCommandService : IProjectCommandService
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectCommandService(IProjectRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
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
