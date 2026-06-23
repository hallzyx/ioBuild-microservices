namespace IoBuild.Devices.Interfaces.REST.Resources;

/// <summary>
/// Metadata for a single controllable attribute on a device type (R-3, task 3.2).
/// Sourced from <c>DeviceCapabilityCatalog.ControllableAttribute</c>.
/// </summary>
/// <param name="Name">Attribute identifier, camelCase (e.g. "targetTemperature").</param>
/// <param name="Type">Value kind: "number", "boolean", "enum".</param>
/// <param name="Min">Minimum allowed value for numeric types; null for non-numeric.</param>
/// <param name="Max">Maximum allowed value for numeric types; null for non-numeric.</param>
/// <param name="Unit">Display unit (e.g. "C", "%"); null when not applicable.</param>
/// <param name="EnumMembers">Allowed values for "enum" types; null/empty for non-enum types.</param>
public record ControllableAttributeResource(
    string Name,
    string Type,
    double? Min,
    double? Max,
    string? Unit,
    IReadOnlyList<string>? EnumMembers = null);

/// <summary>
/// Single entry in the device-type catalog response (S1.1, R-3).
/// </summary>
/// <param name="Code">Canonical type code — used in all contracts (event, command, REST, DB).</param>
/// <param name="DisplayName">Human-readable label for UI rendering.</param>
/// <param name="ControllableAttributes">
/// Attributes the backend can command on this device type (from DeviceCapabilityCatalog).
/// Empty list for telemetry-only types (SmartMeter, WaterSensor, SmokeDetector).
/// </param>
public record DeviceTypeResource(
    string Code,
    string DisplayName,
    IReadOnlyList<ControllableAttributeResource> ControllableAttributes);

/// <summary>
/// Response body for GET /api/v1/devices/types (S1.1, SC-1.1).
/// </summary>
public record DeviceTypeCatalogResource(IReadOnlyList<DeviceTypeResource> DeviceTypes);
