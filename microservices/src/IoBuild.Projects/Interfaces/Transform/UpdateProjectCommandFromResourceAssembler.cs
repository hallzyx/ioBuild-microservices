using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class UpdateProjectCommandFromResourceAssembler
{
    public static UpdateProjectCommand ToCommand(int id, UpdateProjectResource resource)
    {
        return new UpdateProjectCommand(
            id,
            resource.Name,
            resource.Description,
            resource.Location,
            resource.TotalUnits,
            resource.OccupiedUnits,
            resource.Status,
            resource.ImageUrl);
    }
}
