namespace IoBuild.Profiles.Interfaces.REST.Resources;

public record ProfileResource(
    int Id, 
    int UserId, 
    string PhotoUrl, 
    string Name, 
    string Username, 
    string Address, 
    int Age, 
    string PhoneNumber,
    string? SecondEmail);
