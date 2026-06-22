namespace IoBuild.Devices.Domain.Constants;

/// <summary>
/// Catalog of unit-level device types introduced in Change A (unit-device-packages).
/// These types are controllable (see DeviceCapabilityCatalog in IoBuild.Shared) and
/// are provisioned per-unit by UnitDeviceProvisioningConsumer.
///
/// IMPORTANT: These types are intentionally NOT added to FloorDeviceDefaults.Defaults —
/// that set drives the 3 legacy floor-level defaults only (ADR-4).
/// The /types endpoint unions FloorDeviceDefaults.Catalog + UnitDeviceCatalog.Catalog
/// so all 5 types are served to the frontend picker.
/// </summary>
public static class UnitDeviceCatalog
{
    /// <summary>
    /// Unit-level device type entries served by the GET /api/v1/devices/types endpoint.
    /// </summary>
    public static readonly IReadOnlyList<(string Type, string DisplayName)> Catalog =
    [
        ("AirConditioner", "Air Conditioner"),
        ("SmartLight",     "Smart Light"),
    ];
}
