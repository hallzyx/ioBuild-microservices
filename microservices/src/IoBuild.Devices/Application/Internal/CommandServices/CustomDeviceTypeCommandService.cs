using IoBuild.Devices.Application.DTOs;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;

namespace IoBuild.Devices.Application.Internal.CommandServices;

/// <summary>
/// Creates owner-scoped custom device types and persists them via DevicesDbContext.
/// The unique index (owner_user_id, type_code) enforces one type-code per owner;
/// callers must catch <see cref="Microsoft.EntityFrameworkCore.DbUpdateException"/> to surface 409.
/// </summary>
public class CustomDeviceTypeCommandService(DevicesDbContext db)
{
    /// <summary>
    /// Persists a new custom device type for the given owner.
    /// </summary>
    /// <param name="ownerId">Owner's userId (from JWT principal).</param>
    /// <param name="typeCode">Short machine code; unique per owner.</param>
    /// <param name="displayName">Human-readable name (1–100 chars).</param>
    /// <param name="attributes">At least one valid attribute.</param>
    /// <returns>DTO of the persisted record.</returns>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">
    /// Thrown when the (ownerId, typeCode) unique index is violated (duplicate type).
    /// The controller catches this and returns 409.
    /// </exception>
    public async Task<CustomDeviceTypeDto> CreateAsync(
        string ownerId,
        string typeCode,
        string displayName,
        IReadOnlyList<OwnerCustomDeviceTypeAttribute> attributes)
    {
        var entity = new OwnerCustomDeviceType(ownerId, typeCode, displayName, attributes);
        db.OwnerCustomDeviceTypes.Add(entity);
        await db.SaveChangesAsync();

        return CustomDeviceTypeDto.From(entity);
    }
}
