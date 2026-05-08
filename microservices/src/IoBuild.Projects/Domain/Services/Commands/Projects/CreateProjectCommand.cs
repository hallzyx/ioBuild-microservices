namespace IoBuild.Projects.Domain.Services.Commands.Projects;

public class CreateProjectCommand
{
    public string Name { get; }
    public string Description { get; }
    public string Location { get; }
    public int TotalUnits { get; }
    public int BuilderId { get; }
    public string ImageUrl { get; }

    public CreateProjectCommand(string name, string description, string location, int totalUnits, int builderId, string imageUrl)
    {
        Name = name;
        Description = description;
        Location = location;
        TotalUnits = totalUnits;
        BuilderId = builderId;
        ImageUrl = imageUrl;
    }
}
