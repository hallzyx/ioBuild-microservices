using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Devices.Domain.Repositories;

public interface IDeviceRepository : IBaseRepository<Device>
{
    Task<Device?> FindByMacAddressAsync(string macAddress);
    Task<IEnumerable<Device>> FindByProjectIdAsync(int projectId);
    Task SaveChangesAsync();

    /// <summary>
    /// Returns true if any device with the given (projectId, floorNumber, type) already exists.
    /// Used as the idempotency pre-check guard in <see cref="IoBuild.Devices.Infrastructure.Messaging.FloorProvisioningConsumer"/>
    /// (ADR-C, §4.3).
    /// </summary>
    Task<bool> ExistsByProjectFloorTypeAsync(int projectId, int floorNumber, string type);
}
