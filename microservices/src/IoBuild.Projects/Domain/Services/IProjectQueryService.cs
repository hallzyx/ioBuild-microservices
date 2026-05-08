using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Services.Queries.Projects;

namespace IoBuild.Projects.Domain.Services;

public interface IProjectQueryService
{
    Task<IEnumerable<Project>> Handle(GetAllProjectsQuery query);
    Task<Project?> Handle(GetProjectByIdQuery query);
}
