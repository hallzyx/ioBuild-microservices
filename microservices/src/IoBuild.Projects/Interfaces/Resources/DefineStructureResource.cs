namespace IoBuild.Projects.Interfaces.Resources;

/// <summary>
/// REST request body for POST /api/v1/projects/{id}/structure (REQ-PS-01).
/// The REST layer expands floors × unitsPerFloor into explicit FloorSpec lists
/// before handing off to DefineProjectStructureCommand.
/// </summary>
public class DefineStructureResource
{
    /// <summary>Number of floors to create (must be ≥ 1).</summary>
    public int Floors { get; set; }

    /// <summary>Number of units per floor (must be ≥ 1).</summary>
    public int UnitsPerFloor { get; set; }

    /// <summary>
    /// Optional per-unit owner email pre-assignments.
    /// Null or empty means no pre-assignments — all units start with OwnerEmail = null.
    /// </summary>
    public List<OwnerEmailAssignment>? OwnerEmails { get; set; }
}

/// <summary>Identifies a specific unit by floor and room number and assigns an owner email.</summary>
public class OwnerEmailAssignment
{
    public int Floor { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
}
