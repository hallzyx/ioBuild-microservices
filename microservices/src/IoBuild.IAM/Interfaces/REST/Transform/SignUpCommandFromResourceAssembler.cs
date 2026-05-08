using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Interfaces.REST.Transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommand(SignUpResource resource)
    {
        return new SignUpCommand(resource.Email, resource.Password, resource.Role);
    }
}
