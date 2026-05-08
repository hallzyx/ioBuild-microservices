namespace IoBuild.Projects.Domain.Services.Commands.Units;

public class CreateUnitCommand
{
    public int ProjectId { get; }
    public string UnitNumber { get; }
    public int OwnerId { get; }

    public CreateUnitCommand(int projectId, string unitNumber, int ownerId)
    {
        ProjectId = projectId;
        UnitNumber = unitNumber;
        OwnerId = ownerId;
    }
}
