namespace IoBuild.Profiles.Interfaces.REST.Resources;

public class CreateProfileResource
{
    public int UserId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Age { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}
