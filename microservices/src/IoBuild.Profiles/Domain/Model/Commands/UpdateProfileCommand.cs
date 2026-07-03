namespace IoBuild.Profiles.Domain.Model.Commands;

public record UpdateProfileCommand(
    int Id,
    string PhotoUrl,
    string Name,
    string Username,
    string Address,
    int Age,
    string PhoneNumber,
    string? SecondEmail = null);
