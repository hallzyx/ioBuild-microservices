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

    /// <summary>
    /// Optional per-floor device-type selections (S2.1).
    /// Null or absent → all floors use the 3 legacy defaults.
    /// A floor not listed here uses legacy defaults.
    /// A floor listed with empty deviceTypes is treated as "no selection" (uses defaults).
    /// </summary>
    public List<FloorDeviceTypeSelection>? DeviceTypesPerFloor { get; set; }
}

/// <summary>Identifies a specific unit by floor and room number and assigns an owner email.</summary>
public class OwnerEmailAssignment
{
    public int Floor { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// Per-floor device-type selection entry in DefineStructureResource (S2.1).
/// Mirrors the per-floor OwnerEmailAssignment lookup pattern.
/// </summary>
public class FloorDeviceTypeSelection
{
    /// <summary>1-based floor number. Must be within [1, Floors].</summary>
    public int Floor { get; set; }

    /// <summary>Device type codes selected for this floor (e.g., "SmartMeter", "WaterSensor").</summary>
    public List<string> DeviceTypes { get; set; } = new();
}
