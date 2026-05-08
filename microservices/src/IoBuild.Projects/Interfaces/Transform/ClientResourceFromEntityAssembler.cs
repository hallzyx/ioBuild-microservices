using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class ClientResourceFromEntityAssembler
{
    public static ClientResource ToResource(Client entity)
    {
        return new ClientResource
        {
            Id = entity.Id,
            FullName = entity.FullName,
            ProjectId = entity.ProjectId,
            ProjectName = entity.ProjectName,
            AccountStatement = entity.AccountStatement,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            Address = entity.Address
        };
    }

    public static IEnumerable<ClientResource> ToResourceEnumerable(IEnumerable<Client> entities)
    {
        return entities.Select(ToResource);
    }
}
