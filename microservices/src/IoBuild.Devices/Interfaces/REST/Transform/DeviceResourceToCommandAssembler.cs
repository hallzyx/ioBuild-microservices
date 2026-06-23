using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Interfaces.REST.Resources;

namespace IoBuild.Devices.Interfaces.REST.Transform;

public static class DeviceResourceToCommandAssembler
{
    public static CreateDeviceCommand ToCommandFromResource(CreateDeviceResource resource,
        string? requestingOwnerId = null)
    {
        return new CreateDeviceCommand(
            resource.Name,
            resource.Type,
            resource.Location,
            resource.MacAddress,
            resource.ProjectId,
            resource.Status,
            UnitId: resource.UnitId,
            RequestingOwnerId: requestingOwnerId
        );
    }

    public static UpdateDeviceCommand ToCommandFromResource(int id, UpdateDeviceResource resource)
    {
        return new UpdateDeviceCommand(
            id,
            resource.Name,
            resource.Type,
            resource.Location,
            resource.MacAddress,
            resource.ProjectId,
            resource.Status
        );
    }
}
