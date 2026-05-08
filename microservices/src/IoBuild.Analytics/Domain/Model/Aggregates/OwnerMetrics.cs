using IoBuild.Analytics.Domain.Model.Entities;

namespace IoBuild.Analytics.Domain.Model.Aggregates;

public class OwnerMetrics
{
    public int TotalDevices { get; set; }
    public int OnlineDevices { get; set; }
    public int OfflineDevices { get; set; }
    public int AlertsCount { get; set; }
    public int MyUnitsCount { get; set; }
    public double EnergyThisMonth { get; set; }
    public double TemperatureAvg { get; set; }
    public double WaterUsageThisMonth { get; set; }
    public List<HistoricalDataPoint> TemperatureHistory { get; set; } = [];
    public List<HistoricalDataPoint> EnergyHistory { get; set; } = [];
    public List<HistoricalDataPoint> DailyEnergyConsumption { get; set; } = [];
    public List<HistoricalDataPoint> WaterUsageWeekly { get; set; } = [];
    public List<DeviceHealthStatus> DeviceHealthStatus { get; set; } = [];
    public List<Dictionary<string, object>> MyUnitsDetails { get; set; } = [];
}
