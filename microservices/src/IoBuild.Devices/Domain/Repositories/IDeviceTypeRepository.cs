using IoBuild.Devices.Domain.Model.Aggregates;

namespace IoBuild.Devices.Domain.Repositories;

/// <summary>
/// Repository contract for the global device-type catalog.
/// Read-only — catalog is seeded via HasData migration, not mutated at runtime.
/// </summary>
public interface IDeviceTypeRepository
{
    /// <summary>Returns the catalog entry for the given code, or null if not found.</summary>
    Task<DeviceType?> FindByCodeAsync(string code);

    /// <summary>Returns all catalog entries ordered by Code.</summary>
    Task<IReadOnlyList<DeviceType>> ListAllAsync();
}
