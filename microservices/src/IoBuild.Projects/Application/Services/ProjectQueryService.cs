using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Queries.Projects;

namespace IoBuild.Projects.Application.Services;

public class ProjectQueryService : IProjectQueryService
{
    private readonly IProjectRepository _repository;

    public ProjectQueryService(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Project>> Handle(GetAllProjectsQuery query)
    {
        return await _repository.ListAsync();
    }

    public async Task<Project?> Handle(GetProjectByIdQuery query)
    {
        return await _repository.FindByIdAsync(query.Id);
    }
}
