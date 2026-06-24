using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDeviceTypeRepository"/>.
/// Reads from the <c>device_types</c> table seeded by AddDeviceTypeCatalog migration.
/// </summary>
public class DeviceTypeRepository(DevicesDbContext db) : IDeviceTypeRepository
{
    public async Task<DeviceType?> FindByCodeAsync(string code) =>
        await db.DeviceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(dt => dt.Code == code);

    public async Task<IReadOnlyList<DeviceType>> ListAllAsync() =>
        await db.DeviceTypes
            .AsNoTracking()
            .OrderBy(dt => dt.Code)
            .ToListAsync();
}
