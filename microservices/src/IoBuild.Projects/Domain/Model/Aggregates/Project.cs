using IoBuild.Projects.Domain.Model.ValueObjects;

namespace IoBuild.Projects.Domain.Model.Aggregates;

public class Project
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public int TotalUnits { get; private set; }
    public int OccupiedUnits { get; private set; }
    public EProjectStatus Status { get; private set; }
    public int BuilderId { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public string ImageUrl { get; private set; }

    protected Project() { }

    public Project(string name, string description, string location, int totalUnits, int builderId, string imageUrl)
    {
        Name = name;
        Description = description;
        Location = location;
        TotalUnits = totalUnits;
        OccupiedUnits = 0;
        Status = EProjectStatus.Planned;
        BuilderId = builderId;
        CreatedDate = DateTime.UtcNow;
        ImageUrl = imageUrl;
    }

    public void Update(string name, string description, string location, int totalUnits, int occupiedUnits, EProjectStatus status, string imageUrl)
    {
        Name = name;
        Description = description;
        Location = location;
        TotalUnits = totalUnits;
        OccupiedUnits = occupiedUnits;
        Status = status;
        ImageUrl = imageUrl;
    }
}
