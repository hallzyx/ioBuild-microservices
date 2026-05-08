namespace IoBuild.Projects.Domain.Model.Aggregates;

public class Unit
{
    public int Id { get; private set; }
    public int ProjectId { get; private set; }
    public string UnitNumber { get; private set; }
    public int OwnerId { get; private set; }

    protected Unit() { }

    public Unit(int projectId, string unitNumber, int ownerId)
    {
        ProjectId = projectId;
        UnitNumber = unitNumber;
        OwnerId = ownerId;
    }
}
