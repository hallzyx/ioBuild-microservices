using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Interfaces.REST.Transform;

public static class UpdatePasswordCommandFromResourceAssembler
{
    public static UpdatePasswordCommand ToCommand(int userId, UpdatePasswordResource resource)
    {
        return new UpdatePasswordCommand(userId, resource.CurrentPassword, resource.NewPassword);
    }
}
