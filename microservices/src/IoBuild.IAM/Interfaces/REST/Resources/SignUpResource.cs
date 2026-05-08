namespace IoBuild.IAM.Interfaces.REST.Resources;

public record SignUpResource(string Email, string Password, string Role);
public record SignUpCommand(string Email, string Password, string Role);
