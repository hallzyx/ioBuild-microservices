namespace IoBuild.IAM.Interfaces.REST.Resources;

public record SignInResource(string Email, string Password);
public record SignInCommand(string Email, string Password);
