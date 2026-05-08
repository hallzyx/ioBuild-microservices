using IoBuild.Projects.Domain.Services.Commands.Projects;

namespace IoBuild.Projects.Domain.Services;

public interface IProjectCommandService
{
    Task<int> Handle(CreateProjectCommand command);
    Task Handle(UpdateProjectCommand command);
    Task Handle(DeleteProjectCommand command);
}
