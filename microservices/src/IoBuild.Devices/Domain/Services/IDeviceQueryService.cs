using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Queries;

namespace IoBuild.Devices.Domain.Services;

public interface IDeviceQueryService
{
    Task<IEnumerable<Device>> Handle(GetAllDevicesQuery query);
    Task<Device?> Handle(GetDeviceByIdQuery query);
}
