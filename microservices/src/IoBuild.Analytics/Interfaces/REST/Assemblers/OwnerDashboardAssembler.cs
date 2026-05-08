using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Interfaces.REST.Resources;

namespace IoBuild.Analytics.Interfaces.REST.Assemblers;

public static class OwnerDashboardAssembler
{
    public static OwnerDashboardResource ToResource(OwnerMetrics metrics)
    {
        return new OwnerDashboardResource(
            metrics.TotalDevices,
            metrics.OnlineDevices,
            metrics.OfflineDevices,
            metrics.AlertsCount,
            metrics.MyUnitsCount,
            metrics.EnergyThisMonth,
            metrics.TemperatureAvg,
            metrics.WaterUsageThisMonth,
            metrics.TemperatureHistory.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.EnergyHistory.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.DailyEnergyConsumption.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.WaterUsageWeekly.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.DeviceHealthStatus.Select(h => new DeviceHealthStatusResource(h.DeviceId, h.DeviceName, h.Status, h.LastOnline)).ToList(),
            metrics.MyUnitsDetails
        );
    }
}
