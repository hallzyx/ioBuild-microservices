namespace IoBuild.IAM.Interfaces.REST.Resources;

public record UpdatePasswordResource(string CurrentPassword, string NewPassword);
