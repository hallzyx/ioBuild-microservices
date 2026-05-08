using IoBuild.Projects.Domain.Model.ValueObjects;

namespace IoBuild.Projects.Interfaces.Resources;

public class ProjectResource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public EProjectStatus Status { get; set; }
    public int BuilderId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ImageUrl { get; set; }
}
