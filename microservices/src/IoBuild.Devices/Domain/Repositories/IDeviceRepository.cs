using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Devices.Domain.Repositories;

public interface IDeviceRepository : IBaseRepository<Device>
{
    Task<Device?> FindByMacAddressAsync(string macAddress);
    Task<IEnumerable<Device>> FindByProjectIdAsync(int projectId);
    Task SaveChangesAsync();
}
