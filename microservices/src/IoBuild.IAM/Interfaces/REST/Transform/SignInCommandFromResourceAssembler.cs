using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Interfaces.REST.Transform;

public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommand(SignInResource resource)
    {
        return new SignInCommand(resource.Email, resource.Password);
    }
}
