namespace IoBuild.Devices.Interfaces.REST.Resources;

public record CreateDeviceResource(
    string Name,
    string Type,
    string Location,
    string? MacAddress,
    int ProjectId,
    string Status,
    /// <summary>
    /// When set, creates the device as OwnerCustom type and assigns it to this unit.
    /// The server validates the caller owns this unit (returns 403 otherwise).
    /// </summary>
    int? UnitId = null);
