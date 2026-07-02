using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Interfaces.REST.Resources;

namespace IoBuild.Devices.Interfaces.REST.Transform;

public static class TelemetryResourceAssembler
{
    public static EnergyDataResource ToEnergyResource(EnergyDataPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return new EnergyDataResource(point.Timestamp, point.EnergyKwh, point.TemperatureC, point.VoltageV);
    }

    public static DeviceStatusResource ToStatusResource(DeviceStatusReport report, Dictionary<string, object>? desired = null)
    {
        ArgumentNullException.ThrowIfNull(report);
        return new DeviceStatusResource(
            report.DeviceId,
            report.Status,
            report.LastSeen,
            report.TemperatureC,
            report.VoltageV,
            desired);
    }
}
