using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Interfaces.REST.Transform;

public static class AuthenticatedUserResourceFromEntityAssembler
{
    public static AuthenticatedUserResource ToResource(User user, string token)
    {
        return new AuthenticatedUserResource(user.Id, user.Email, user.Role, token);
    }
}
