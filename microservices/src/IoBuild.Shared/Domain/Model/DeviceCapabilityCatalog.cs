namespace IoBuild.Shared.Domain.Model;

/// <summary>
/// Metadata-only catalog declaring per-type capabilities for known device types.
/// This class contains NO runtime behavior, NO DB access, and NO validation logic.
/// It exists as a compile-time contract for Change B (owner actuation) with zero further
/// Shared churn — consuming services read this dictionary directly (ADR-5/D5).
///
/// Telemetry-only types (SmartMeter, WaterSensor, SmokeDetector) are intentionally absent
/// from <see cref="ByType"/> — <see cref="IsControllable"/> returns false for them.
/// </summary>
public static class DeviceCapabilityCatalog
{
    /// <summary>
    /// A single controllable attribute on a device type.
    /// </summary>
    /// <param name="Name">Attribute identifier, camelCase (e.g. "targetTemperature").</param>
    /// <param name="Type">Value kind: "number", "boolean", "enum".</param>
    /// <param name="Min">Minimum allowed value for numeric types; null for non-numeric.</param>
    /// <param name="Max">Maximum allowed value for numeric types; null for non-numeric.</param>
    /// <param name="Unit">Display unit (e.g. "C", "%"); null when not applicable.</param>
    /// <param name="EnumMembers">Allowed values for "enum" types; null/empty for non-enum types.</param>
    public record ControllableAttribute(
        string Name,
        string Type,
        double? Min,
        double? Max,
        string? Unit,
        IReadOnlyList<string>? EnumMembers = null);

    /// <summary>
    /// Capability declaration for a device type.
    /// </summary>
    /// <param name="Metrics">Field names reported by the device in telemetry (read-only).</param>
    /// <param name="ControllableAttributes">Attributes the backend can command on the device.</param>
    public record Capability(
        IReadOnlyList<string> Metrics,
        IReadOnlyList<ControllableAttribute> ControllableAttributes);

    /// <summary>
    /// Per-type capability declarations. Keyed by device type code (Ordinal, matches KnownTypes).
    /// Only controllable types appear here; telemetry-only types are absent by design.
    /// </summary>
    // TODO: Kept alive for Slice 2 migration — do NOT remove until Projects/FloorProvisioningConsumer
    // migrated to read from the device_types DB catalog. KnownTypes static dict also stays until then.
    public static readonly IReadOnlyDictionary<string, Capability> ByType =
        new Dictionary<string, Capability>(StringComparer.Ordinal)
        {
            ["AirConditioner"] = new(
                Metrics: ["temperature_c", "status"],
                ControllableAttributes:
                [
                    new("targetTemperature", "number", 16, 30, "C"),
                    new("mode",              "enum",   null, null, null, ["cooling", "heating", "fan"]),
                    new("power",             "boolean", null, null, null),
                ]),

            ["SmartLight"] = new(
                Metrics: ["status"],
                ControllableAttributes:
                [
                    new("brightness", "number",  0, 100, "%"),
                    new("power",      "boolean", null, null, null),
                ]),
        };

    /// <summary>
    /// Returns true if the given device type has controllable attributes in this catalog.
    /// Telemetry-only types (SmartMeter, WaterSensor, SmokeDetector) return false.
    /// </summary>
    public static bool IsControllable(string type) => ByType.ContainsKey(type);
}
