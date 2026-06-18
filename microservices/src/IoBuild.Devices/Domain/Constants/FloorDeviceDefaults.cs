namespace IoBuild.Devices.Domain.Constants;

/// <summary>
/// Hard-coded default device set provisioned per floor when a FloorStructureDefinedEvent is received
/// (REQ-FD-01, §4.2). Changing the set requires a code change — no runtime configuration.
/// </summary>
public static class FloorDeviceDefaults
{
    /// <summary>
    /// The three default device types created per floor.
    /// Type: the device Type string (used as the EF column value and the unique-index component).
    /// NamePrefix: the human-readable name prefix (displayed in the Name field).
    /// </summary>
    public static readonly IReadOnlyList<(string Type, string NamePrefix)> Defaults =
    [
        ("SmartMeter",    "Smart Meter"),
        ("WaterSensor",   "Water Sensor"),
        ("SmokeDetector", "Smoke Detector"),
    ];
}
