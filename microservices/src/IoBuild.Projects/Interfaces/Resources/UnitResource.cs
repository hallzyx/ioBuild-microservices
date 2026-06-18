namespace IoBuild.Projects.Interfaces.Resources;

public class UnitResource
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string UnitNumber { get; set; }
    public int Floor { get; set; }
    public string RoomNumber { get; set; }
    public string? OwnerEmail { get; set; }
    public int? OwnerId { get; set; }
}
