namespace IoBuild.Shared.Domain.Model;

/// <summary>
/// Canonical device type codes shared between IoBuild.Projects (validation)
/// and IoBuild.Devices (provisioning). Single source of truth in Shared so
/// neither service needs to reference the other (S2.3 / design ADR-2).
/// </summary>
public static class DeviceTypeCatalog
{
    /// <summary>
    /// The set of valid device type codes. Any code not in this set is rejected
    /// by the define-structure endpoint with HTTP 400 (S2.3 / S2.4).
    /// </summary>
    public static readonly IReadOnlySet<string> KnownTypes =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "SmartMeter",
            "WaterSensor",
            "SmokeDetector",
            // Unit-level controllable device types — added for Change A (unit-device-packages).
            // These MUST NOT be added to FloorDeviceDefaults.Defaults (floor legacy defaults only — ADR-4).
            "AirConditioner",
            "SmartLight",
        };
}
