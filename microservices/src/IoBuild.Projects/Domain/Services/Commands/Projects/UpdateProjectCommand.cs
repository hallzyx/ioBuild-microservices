using IoBuild.Projects.Domain.Model.ValueObjects;

namespace IoBuild.Projects.Domain.Services.Commands.Projects;

public class UpdateProjectCommand
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Location { get; }
    public int TotalUnits { get; }
    public int OccupiedUnits { get; }
    public EProjectStatus Status { get; }
    public string ImageUrl { get; }

    public UpdateProjectCommand(int id, string name, string description, string location, int totalUnits, int occupiedUnits, EProjectStatus status, string imageUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        Location = location;
        TotalUnits = totalUnits;
        OccupiedUnits = occupiedUnits;
        Status = status;
        ImageUrl = imageUrl;
    }
}
