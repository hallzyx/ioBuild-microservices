using IoBuild.Analytics.Domain.Model.Entities;

namespace IoBuild.Analytics.Domain.Model.Aggregates;

public class BuilderMetrics
{
    public int TotalDevices { get; set; }
    public int OnlineDevices { get; set; }
    public int OfflineDevices { get; set; }
    public int AlertsCount { get; set; }
    public int ActiveProjectsCount { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public double OccupancyRate { get; set; }
    public double EnergyEfficiencyAvg { get; set; }
    public List<HistoricalDataPoint> TemperatureHistory { get; set; } = [];
    public List<HistoricalDataPoint> EnergyHistory { get; set; } = [];
    public List<HistoricalDataPoint> HourlyEnergyData { get; set; } = [];
    public List<HistoricalDataPoint> MonthlyOccupancy { get; set; } = [];
    public Dictionary<string, int> DevicesByType { get; set; } = [];
    public Dictionary<string, object> ProjectsOverview { get; set; } = [];
}
