namespace IoBuild.Devices.Domain.Constants;

/// <summary>
/// Hard-coded default device set provisioned per floor when a FloorStructureDefinedEvent is received
/// (REQ-FD-01, §4.2). Changing the set requires a code change — no runtime configuration.
///
/// Relationship to DeviceTypeCatalog (IoBuild.Shared):
///   DeviceTypeCatalog.KnownTypes is the authoritative vocabulary (used for validation in Projects).
///   FloorDeviceDefaults.Defaults is the default SELECTION — the 3 types provisioned when no
///   explicit DeviceTypes are provided. Both sets are identical today; KnownTypes drives the
///   catalog endpoint (single source of truth for vocabulary/display names).
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

    /// <summary>
    /// Catalog accessor: maps Defaults to (Type, DisplayName) tuples used by the
    /// GET /api/v1/devices/types endpoint. DisplayName equals NamePrefix.
    /// </summary>
    public static IReadOnlyList<(string Type, string DisplayName)> Catalog =>
        Defaults.Select(d => (d.Type, d.NamePrefix)).ToList();
}
