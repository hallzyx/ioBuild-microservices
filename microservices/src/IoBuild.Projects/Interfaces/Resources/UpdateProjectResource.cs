using IoBuild.Projects.Domain.Model.ValueObjects;

namespace IoBuild.Projects.Interfaces.Resources;

public class UpdateProjectResource
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public EProjectStatus Status { get; set; }
    public string ImageUrl { get; set; }
}
