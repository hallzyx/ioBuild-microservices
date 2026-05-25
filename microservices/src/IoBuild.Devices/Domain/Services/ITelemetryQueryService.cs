using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Queries;

namespace IoBuild.Devices.Domain.Services;

public interface ITelemetryQueryService
{
    Task<IEnumerable<EnergyDataPoint>> Handle(GetDeviceEnergyQuery query);
    Task<DeviceStatusReport?> Handle(GetDeviceStatusQuery query);
}
