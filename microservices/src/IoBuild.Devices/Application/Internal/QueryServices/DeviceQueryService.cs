using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Queries;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Domain.Services;

namespace IoBuild.Devices.Application.Internal.QueryServices;

public class DeviceQueryService(IDeviceRepository repository) : IDeviceQueryService
{
    public async Task<IEnumerable<Device>> Handle(GetAllDevicesQuery query)
    {
        return await repository.ListAsync();
    }

    public async Task<Device?> Handle(GetDeviceByIdQuery query)
    {
        return await repository.FindByIdAsync(query.Id);
    }
}
