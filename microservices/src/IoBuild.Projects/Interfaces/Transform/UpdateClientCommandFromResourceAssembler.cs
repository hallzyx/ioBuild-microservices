using IoBuild.Projects.Domain.Services.Commands.Clients;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class UpdateClientCommandFromResourceAssembler
{
    public static UpdateClientCommand ToCommand(int id, UpdateClientResource resource)
    {
        return new UpdateClientCommand(
            id,
            resource.FullName,
            resource.ProjectName,
            resource.AccountStatement,
            resource.Email,
            resource.PhoneNumber,
            resource.Address);
    }
}
