using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Devices.Domain.Repositories;

public interface IDeviceLogRepository : IBaseRepository<DeviceLog>
{
    Task<IEnumerable<DeviceLog>> FindByDeviceIdAsync(int deviceId);
    Task<IEnumerable<DeviceLog>> FindByProjectIdAsync(int projectId);
    Task<IEnumerable<DeviceLog>> FindByTypeAsync(string type);
    Task<IEnumerable<DeviceLog>> FindByDateRangeAsync(DateTime start, DateTime end);
}
