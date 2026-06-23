using IoBuild.Devices.Domain.Model.Aggregates;

namespace IoBuild.Devices.Application.DTOs;

/// <summary>
/// Read-side DTO for a persisted owner custom device type.
/// </summary>
public record CustomDeviceTypeDto(
    int Id,
    string OwnerUserId,
    string TypeCode,
    string DisplayName,
    IReadOnlyList<OwnerCustomDeviceTypeAttribute> Attributes)
{
    public static CustomDeviceTypeDto From(OwnerCustomDeviceType entity) =>
        new(entity.Id, entity.OwnerUserId, entity.TypeCode, entity.DisplayName, entity.GetAttributes());
}
