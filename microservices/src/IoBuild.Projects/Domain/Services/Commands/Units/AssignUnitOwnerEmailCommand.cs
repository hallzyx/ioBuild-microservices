namespace IoBuild.Projects.Domain.Services.Commands.Units;

public class AssignUnitOwnerEmailCommand
{
    public int UnitId { get; }
    public string? OwnerEmail { get; }

    public AssignUnitOwnerEmailCommand(int unitId, string? ownerEmail)
    {
        UnitId = unitId;
        OwnerEmail = ownerEmail;
    }
}
