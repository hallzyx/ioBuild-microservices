namespace IoBuild.Projects.Interfaces.Resources;

public class CreateProjectResource
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public int TotalUnits { get; set; }
    public int BuilderId { get; set; }
    public string ImageUrl { get; set; }
}
