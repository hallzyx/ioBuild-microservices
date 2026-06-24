using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Interfaces.REST.Resources;

namespace IoBuild.Devices.Application.Internal.QueryServices;

/// <summary>
/// Query service for the global device-type catalog.
/// Maps DB catalog rows to the existing <see cref="DeviceTypeResource"/> DTO shape
/// (Code, DisplayName, ControllableAttributes — NO Scope per design D5).
/// </summary>
public class DeviceTypeCatalogQueryService(IDeviceTypeRepository repository)
{
    /// <summary>
    /// Returns all catalog rows mapped to <see cref="DeviceTypeResource"/>.
    /// Telemetry-only types (SmartMeter, WaterSensor, SmokeDetector) have an empty
    /// <see cref="DeviceTypeResource.ControllableAttributes"/> list.
    /// </summary>
    public async Task<IReadOnlyList<DeviceTypeResource>> ListAllAsync()
    {
        var rows = await repository.ListAllAsync();

        return rows.Select(dt =>
        {
            var attrs = dt.GetAttributes()
                .Select(a => new ControllableAttributeResource(
                    a.Name, a.Type, a.Min, a.Max, a.Unit, a.EnumMembers))
                .ToList();

            return new DeviceTypeResource(dt.Code, dt.DisplayName, attrs);
        }).ToList();
    }
}
