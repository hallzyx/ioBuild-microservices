using IoBuild.Projects.Domain.Services.Commands.Clients;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class CreateClientCommandFromResourceAssembler
{
    public static CreateClientCommand ToCommand(CreateClientResource resource)
    {
        return new CreateClientCommand(
            resource.FullName,
            resource.ProjectId,
            resource.ProjectName,
            resource.Email,
            resource.PhoneNumber,
            resource.Address);
    }
}
