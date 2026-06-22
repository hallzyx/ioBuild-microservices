namespace IoBuild.Devices.Interfaces.REST.Resources;

/// <summary>
/// Single entry in the device-type catalog response (S1.1).
/// </summary>
/// <param name="Code">Canonical type code — used in all contracts (event, command, REST, DB).</param>
/// <param name="DisplayName">Human-readable label for UI rendering.</param>
public record DeviceTypeResource(string Code, string DisplayName);

/// <summary>
/// Response body for GET /api/v1/devices/types (S1.1, SC-1.1).
/// </summary>
public record DeviceTypeCatalogResource(IReadOnlyList<DeviceTypeResource> DeviceTypes);
