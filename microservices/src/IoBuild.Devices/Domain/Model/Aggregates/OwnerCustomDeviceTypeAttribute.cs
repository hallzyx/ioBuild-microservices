namespace IoBuild.Devices.Domain.Model.Aggregates;

/// <summary>
/// Value object describing a single controllable attribute on an owner-defined custom device type.
/// Mirrors <see cref="IoBuild.Shared.Domain.Model.DeviceCapabilityCatalog.ControllableAttribute"/> shape
/// but lives in the Devices domain for the custom-type aggregate.
/// </summary>
public record OwnerCustomDeviceTypeAttribute(
    string Name,
    string Type,
    double? Min = null,
    double? Max = null,
    string? Unit = null,
    IReadOnlyList<string>? EnumMembers = null);
