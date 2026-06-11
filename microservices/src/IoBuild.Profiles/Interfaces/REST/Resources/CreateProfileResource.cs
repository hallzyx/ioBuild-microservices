namespace IoBuild.Profiles.Interfaces.REST.Resources;

public record CreateProfileResource(
    int UserId, 
    string PhotoUrl, 
    string Name, 
    string Username, 
    string Address, 
    int Age, 
    string PhoneNumber);
