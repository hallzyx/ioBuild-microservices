using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Interfaces.REST.Resources;

namespace IoBuild.Analytics.Interfaces.REST.Assemblers;

public static class BuilderDashboardAssembler
{
    public static BuilderDashboardResource ToResource(BuilderMetrics metrics)
    {
        return new BuilderDashboardResource(
            metrics.TotalDevices,
            metrics.OnlineDevices,
            metrics.OfflineDevices,
            metrics.AlertsCount,
            metrics.ActiveProjectsCount,
            metrics.TotalUnits,
            metrics.OccupiedUnits,
            metrics.OccupancyRate,
            metrics.EnergyEfficiencyAvg,
            metrics.TemperatureHistory.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.EnergyHistory.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.HourlyEnergyData.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.MonthlyOccupancy.Select(HistoricalDataPointAssembler.ToResource).ToList(),
            metrics.DevicesByType,
            metrics.ProjectsOverview
        );
    }
}
