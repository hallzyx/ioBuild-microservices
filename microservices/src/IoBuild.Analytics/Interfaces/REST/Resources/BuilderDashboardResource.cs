namespace IoBuild.Analytics.Interfaces.REST.Resources;

public record BuilderDashboardResource(
    int TotalDevices,
    int OnlineDevices,
    int OfflineDevices,
    int AlertsCount,
    int ActiveProjectsCount,
    int TotalUnits,
    int OccupiedUnits,
    double OccupancyRate,
    double EnergyEfficiencyAvg,
    List<HistoricalDataPointResource> TemperatureHistory,
    List<HistoricalDataPointResource> EnergyHistory,
    List<HistoricalDataPointResource> HourlyEnergyData,
    List<HistoricalDataPointResource> MonthlyOccupancy,
    Dictionary<string, int> DevicesByType,
    List<Dictionary<string, object>> ProjectsOverview
);
