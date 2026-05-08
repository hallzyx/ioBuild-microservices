using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class CreateProjectCommandFromResourceAssembler
{
    public static CreateProjectCommand ToCommand(CreateProjectResource resource)
    {
        return new CreateProjectCommand(
            resource.Name,
            resource.Description,
            resource.Location,
            resource.TotalUnits,
            resource.BuilderId,
            resource.ImageUrl);
    }
}
