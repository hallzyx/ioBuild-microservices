namespace IoBuild.Devices.Interfaces.REST.Resources;

/// <summary>
/// Request body for POST /api/v1/devices/types/custom.
/// ownerId is NOT accepted from the body — it is always derived from the JWT principal.
/// </summary>
public record CreateCustomTypeResource(
    string TypeCode,
    string DisplayName,
    IReadOnlyList<CustomTypeAttributeResource> Attributes);

/// <summary>
/// One controllable attribute in a custom device type definition.
/// </summary>
public record CustomTypeAttributeResource(
    string Name,
    string Type,
    double? Min,
    double? Max,
    string? Unit,
    IReadOnlyList<string>? EnumMembers);
