namespace IoBuild.Devices.Domain.Model.Commands;

public record CreateDeviceCommand(
    string Name,
    string Type,
    string Location,
    string? MacAddress,
    int ProjectId,
    string Status,
    /// <summary>
    /// When set, creates the device as OwnerCustom and assigns it to this unit.
    /// The caller (controller) must validate ownership before dispatching this command.
    /// </summary>
    int? UnitId = null,
    /// <summary>String userId of the calling owner; required when UnitId is set.</summary>
    string? RequestingOwnerId = null);
