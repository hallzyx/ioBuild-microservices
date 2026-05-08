using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Interfaces.REST.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResource(User user)
    {
        return new UserResource(user.Id, user.Email, user.Role);
    }

    public static IEnumerable<UserResource> ToResourceList(IEnumerable<User> users)
    {
        return users.Select(ToResource);
    }
}
