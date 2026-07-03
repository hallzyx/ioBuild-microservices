namespace IoBuild.Profiles.Interfaces.REST.Resources;

public record UpdateProfileResource(
    string PhotoUrl,
    string Name,
    string Username,
    string Address,
    int Age,
    string PhoneNumber,
    string? SecondEmail = null);
