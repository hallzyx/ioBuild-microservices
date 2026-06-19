using IoBuild.Projects.Domain.Services.Commands.Units;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class AssignUnitOwnerCommandFromResourceAssembler
{
    public static AssignUnitOwnerEmailCommand ToCommand(int unitId, AssignUnitOwnerResource resource)
    {
        return new AssignUnitOwnerEmailCommand(unitId, resource.OwnerEmail);
    }
}
