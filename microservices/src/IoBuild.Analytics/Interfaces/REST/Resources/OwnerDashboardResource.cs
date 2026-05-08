namespace IoBuild.Analytics.Interfaces.REST.Resources;

public record OwnerDashboardResource(
    int TotalDevices,
    int OnlineDevices,
    int OfflineDevices,
    int AlertsCount,
    int MyUnitsCount,
    double EnergyThisMonth,
    double TemperatureAvg,
    double WaterUsageThisMonth,
    List<HistoricalDataPointResource> TemperatureHistory,
    List<HistoricalDataPointResource> EnergyHistory,
    List<HistoricalDataPointResource> DailyEnergyConsumption,
    List<HistoricalDataPointResource> WaterUsageWeekly,
    List<DeviceHealthStatusResource> DeviceHealthStatus,
    List<Dictionary<string, object>> MyUnitsDetails
);

public record DeviceHealthStatusResource(
    int DeviceId,
    string DeviceName,
    string Status,
    DateTime LastOnline
);
