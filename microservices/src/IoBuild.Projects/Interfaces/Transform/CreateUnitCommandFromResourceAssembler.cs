using IoBuild.Projects.Domain.Services.Commands.Units;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class CreateUnitCommandFromResourceAssembler
{
    public static CreateUnitCommand ToCommand(CreateUnitResource resource)
    {
        return new CreateUnitCommand(
            resource.ProjectId,
            resource.UnitNumber,
            resource.OwnerId);
    }
}
