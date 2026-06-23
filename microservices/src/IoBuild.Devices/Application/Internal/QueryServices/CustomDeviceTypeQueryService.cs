using IoBuild.Devices.Application.DTOs;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Application.Internal.QueryServices;

/// <summary>
/// Returns custom device types scoped to the requesting owner.
/// </summary>
public class CustomDeviceTypeQueryService(DevicesDbContext db)
{
    /// <summary>
    /// Returns all custom device types defined by the given owner.
    /// Returns an empty list when none exist.
    /// </summary>
    public async Task<IReadOnlyList<CustomDeviceTypeDto>> ListByOwnerAsync(string ownerId)
    {
        var rows = await db.OwnerCustomDeviceTypes
            .Where(t => t.OwnerUserId == ownerId)
            .ToListAsync();

        return rows.Select(CustomDeviceTypeDto.From).ToList();
    }
}
