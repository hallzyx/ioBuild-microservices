using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Interfaces.REST.Resources;

namespace IoBuild.Devices.Interfaces.REST.Transform;

public static class DeviceResourceFromEntityAssembler
{
    public static DeviceResource ToResourceFromEntity(Device entity)
    {
        return new DeviceResource(
            entity.Id,
            entity.Name,
            entity.Type,
            entity.Location,
            entity.MacAddress,
            entity.ProjectId,
            entity.Status
        );
    }
}
