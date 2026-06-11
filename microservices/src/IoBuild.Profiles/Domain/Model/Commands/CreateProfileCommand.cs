namespace IoBuild.Profiles.Domain.Model.Commands;

public record CreateProfileCommand(
    int UserId, 
    string PhotoUrl, 
    string Name, 
    string Username, 
    string Address, 
    int Age, 
    string PhoneNumber);
