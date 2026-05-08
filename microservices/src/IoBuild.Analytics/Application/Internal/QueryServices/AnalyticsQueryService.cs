using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Domain.Model.Entities;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Domain.Services;
using IoBuild.Analytics.Interfaces.ACL;

namespace IoBuild.Analytics.Application.Internal.QueryServices;

public class AnalyticsQueryService : IAnalyticsQueryService
{
    private readonly IDevicesContextFacade _devices;
    private readonly IProjectsContextFacade _projects;
    private readonly ILogger<AnalyticsQueryService> _logger;

    public AnalyticsQueryService(
        IDevicesContextFacade devices,
        IProjectsContextFacade projects,
        ILogger<AnalyticsQueryService> logger)
    {
        _devices = devices;
        _projects = projects;
        _logger = logger;
    }

    public async Task<BuilderMetrics?> Handle(GetBuilderDashboardQuery query)
    {
        _logger.LogInformation("Building builder dashboard for user {UserId}", query.UserId);

        var totalDevices = await _devices.GetTotalDevicesAsync(query.UserId);
        var onlineDevices = await _devices.GetOnlineDevicesAsync(query.UserId);
        var offlineDevices = await _devices.GetOfflineDevicesAsync(query.UserId);
        var alertsCount = await _devices.GetAlertsCountAsync(query.UserId);
        var activeProjects = await _projects.GetActiveProjectsCountAsync(query.UserId);
        var totalUnits = await _projects.GetTotalUnitsAsync(query.UserId);
        var occupiedUnits = await _projects.GetOccupiedUnitsAsync(query.UserId);
        var occupancyRate = await _projects.GetOccupancyRateAsync(query.UserId);
        var energyEfficiency = await _projects.GetEnergyEfficiencyAverageAsync(query.UserId);
        var devicesByType = await _devices.GetDevicesByTypeAsync(query.UserId);
        var occupancyHistory = await _projects.GetOccupancyHistoryAsync(query.UserId);

        return new BuilderMetrics
        {
            TotalDevices = totalDevices,
            OnlineDevices = onlineDevices,
            OfflineDevices = offlineDevices,
            AlertsCount = alertsCount,
            ActiveProjectsCount = activeProjects,
            TotalUnits = totalUnits,
            OccupiedUnits = occupiedUnits,
            OccupancyRate = occupancyRate,
            EnergyEfficiencyAvg = energyEfficiency,
            TemperatureHistory = MapToDataPoints(occupancyHistory, "temperature"),
            EnergyHistory = MapToDataPoints(occupancyHistory, "energy"),
            HourlyEnergyData = MapToDataPoints(occupancyHistory, "hourly_energy"),
            MonthlyOccupancy = MapToDataPoints(occupancyHistory, "monthly_occupancy"),
            DevicesByType = devicesByType,
            ProjectsOverview = MapProjectsOverview(activeProjects, totalUnits, occupiedUnits)
        };
    }

    public async Task<OwnerMetrics?> Handle(GetOwnerDashboardQuery query)
    {
        _logger.LogInformation("Building owner dashboard for user {UserId}", query.UserId);

        var totalDevices = await _devices.GetTotalDevicesAsync(query.UserId);
        var onlineDevices = await _devices.GetOnlineDevicesAsync(query.UserId);
        var offlineDevices = await _devices.GetOfflineDevicesAsync(query.UserId);
        var alertsCount = await _devices.GetAlertsCountAsync(query.UserId);
        var myUnitsCount = await _projects.GetMyUnitsCountAsync(query.UserId);
        var energyThisMonth = await _devices.GetEnergyThisMonthAsync(query.UserId);
        var temperatureAvg = await _devices.GetTemperatureAverageAsync(query.UserId);
        var waterUsage = await _devices.GetWaterUsageThisMonthAsync(query.UserId);
        var healthStatuses = await _devices.GetDeviceHealthStatusesAsync(query.UserId);
        var unitsDetails = await _projects.GetOwnerUnitsDetailsAsync(query.UserId);

        return new OwnerMetrics
        {
            TotalDevices = totalDevices,
            OnlineDevices = onlineDevices,
            OfflineDevices = offlineDevices,
            AlertsCount = alertsCount,
            MyUnitsCount = myUnitsCount,
            EnergyThisMonth = energyThisMonth,
            TemperatureAvg = temperatureAvg,
            WaterUsageThisMonth = waterUsage,
            TemperatureHistory = MapToDataPoints(unitsDetails, "temperature"),
            EnergyHistory = MapToDataPoints(unitsDetails, "energy"),
            DailyEnergyConsumption = MapToDataPoints(unitsDetails, "daily_energy"),
            WaterUsageWeekly = MapToDataPoints(unitsDetails, "water_weekly"),
            DeviceHealthStatus = healthStatuses.Select(h => new DeviceHealthStatus
            {
                DeviceId = Convert.ToInt32(h.GetValueOrDefault("deviceId", 0)),
                DeviceName = h.GetValueOrDefault("deviceName")?.ToString() ?? "",
                Status = h.GetValueOrDefault("status")?.ToString() ?? "unknown",
                LastOnline = DateTime.TryParse(h.GetValueOrDefault("lastOnline")?.ToString(), out var dt) ? dt : DateTime.MinValue
            }).ToList(),
            MyUnitsDetails = unitsDetails.ToList()
        };
    }

    public async Task<IEnumerable<HistoricalDataPoint>> Handle(GetHistoricalDataQuery query)
    {
        _logger.LogInformation("Fetching historical data for project {ProjectId}, metric {Metric}", query.ProjectId, query.Metric);

        var telemetry = await _devices.GetDeviceTelemetryAsync(query.ProjectId, query.Metric, query.StartDate, query.EndDate);
        if (telemetry == null)
            return [];

        return telemetry.Select(kvp => new HistoricalDataPoint
        {
            Timestamp = DateTime.TryParse(kvp.Key, out var ts) ? ts : DateTime.MinValue,
            Value = Convert.ToDouble(kvp.Value),
            Metric = query.Metric
        });
    }

    private static List<HistoricalDataPoint> MapToDataPoints(IEnumerable<Dictionary<string, object>> source, string metricKey)
    {
        var points = new List<HistoricalDataPoint>();
        foreach (var item in source)
        {
            if (item.TryGetValue(metricKey, out var value))
            {
                points.Add(new HistoricalDataPoint
                {
                    Timestamp = DateTime.TryParse(item.GetValueOrDefault("timestamp")?.ToString(), out var ts) ? ts : DateTime.UtcNow,
                    Value = Convert.ToDouble(value),
                    Metric = metricKey
                });
            }
        }
        return points;
    }

    private static Dictionary<string, object> MapProjectsOverview(int activeProjects, int totalUnits, int occupiedUnits)
    {
        return new Dictionary<string, object>
        {
            { "activeProjects", activeProjects },
            { "totalUnits", totalUnits },
            { "occupiedUnits", occupiedUnits },
            { "vacantUnits", totalUnits - occupiedUnits }
        };
    }
}
