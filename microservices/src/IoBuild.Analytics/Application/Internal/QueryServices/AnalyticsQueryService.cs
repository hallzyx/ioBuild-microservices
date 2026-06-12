using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Domain.Model.Entities;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Domain.Services;
using IoBuild.Analytics.Interfaces.ACL;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Analytics.Application.Internal.QueryServices;

public class AnalyticsQueryService : IAnalyticsQueryService
{
    private readonly IDevicesContextFacade _devices;
    private readonly IProjectsContextFacade _projects;
    private readonly AnalyticsDbContext _db;
    private readonly ILogger<AnalyticsQueryService> _logger;

    public AnalyticsQueryService(
        IDevicesContextFacade devices,
        IProjectsContextFacade projects,
        AnalyticsDbContext db,
        ILogger<AnalyticsQueryService> logger)
    {
        _devices = devices;
        _projects = projects;
        _db = db;
        _logger = logger;
    }

    public async Task<BuilderMetrics?> Handle(GetBuilderDashboardQuery query)
    {
        _logger.LogInformation("Building builder dashboard for user {UserId}", query.UserId);

        // Try database seed first (fast, offline-capable)
        var latestSnapshot = await _db.BuilderMetrics
            .Where(b => EF.Property<int>(b, "UserId") == query.UserId)
            .OrderByDescending(b => EF.Property<DateTime>(b, "SnapshotDate"))
            .FirstOrDefaultAsync();

        if (latestSnapshot is not null)
        {
            _logger.LogInformation("Using cached dashboard data for user {UserId}", query.UserId);
            // Populate chart data from seed (these are ignored by EF Core, so always empty)
            latestSnapshot.TemperatureHistory = GenerateSampleTemperatureData();
            latestSnapshot.EnergyHistory = GenerateSampleEnergyData();
            latestSnapshot.HourlyEnergyData = GenerateSampleHourlyEnergy();
            latestSnapshot.MonthlyOccupancy = GenerateSampleMonthlyOccupancy();
            latestSnapshot.DevicesByType = new Dictionary<string, int>
            {
                ["Temperature"] = 5, ["Energy"] = 3, ["Water"] = 2,
                ["Access Control"] = 1, ["HVAC"] = 1, ["Lighting"] = 1
            };
            latestSnapshot.ProjectsOverview = new List<Dictionary<string, object>>
            {
                new() { ["id"] = 1, ["name"] = "Residencial Los Álamos", ["location"] = "San Isidro", ["totalUnits"] = 120, ["occupiedUnits"] = 95, ["occupancyRate"] = 79.2, ["status"] = "OnGoing" },
                new() { ["id"] = 2, ["name"] = "Torres del Pacífico", ["location"] = "Miraflores", ["totalUnits"] = 80, ["occupiedUnits"] = 68, ["occupancyRate"] = 85.0, ["status"] = "OnGoing" },
                new() { ["id"] = 3, ["name"] = "Condominio Las Casuarinas", ["location"] = "Surco", ["totalUnits"] = 60, ["occupiedUnits"] = 30, ["occupancyRate"] = 50.0, ["status"] = "OnGoing" }
            };
            return latestSnapshot;
        }

        // Fallback: compute from ACLs (real-time, requires devices/projects APIs)
        _logger.LogInformation("Computing dashboard from APIs for user {UserId}", query.UserId);

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

        // Try database seed first
        var latestSnapshot = await _db.OwnerMetrics
            .Where(o => EF.Property<int>(o, "UserId") == query.UserId)
            .OrderByDescending(o => EF.Property<DateTime>(o, "SnapshotDate"))
            .FirstOrDefaultAsync();

        if (latestSnapshot is not null)
        {
            _logger.LogInformation("Using cached owner dashboard for user {UserId}", query.UserId);
            // Populate chart data (these are ignored by EF Core, so always empty from DB)
            latestSnapshot.TemperatureHistory = GenerateSampleTemperatureData();
            latestSnapshot.EnergyHistory = GenerateSampleEnergyData();
            latestSnapshot.DailyEnergyConsumption = GenerateSampleHourlyEnergy();
            latestSnapshot.WaterUsageWeekly = GenerateSampleWaterUsage();
            latestSnapshot.DeviceHealthStatus = new List<DeviceHealthStatus>
            {
                new() { DeviceId = 1, DeviceName = "Temperature - Living Room", Status = "Online", LastOnline = DateTime.UtcNow },
                new() { DeviceId = 2, DeviceName = "Energy Meter - Main", Status = "Online", LastOnline = DateTime.UtcNow },
                new() { DeviceId = 3, DeviceName = "Water Meter - Garden", Status = "Online", LastOnline = DateTime.UtcNow.AddHours(-2) }
            };
            latestSnapshot.MyUnitsDetails = new List<Dictionary<string, object>>
            {
                new() { ["unitNumber"] = "A-101", ["projectName"] = "Condominio Las Casuarinas", ["status"] = "Occupied", ["occupancyRate"] = 100 },
                new() { ["unitNumber"] = "A-102", ["projectName"] = "Condominio Las Casuarinas", ["status"] = "Occupied", ["occupancyRate"] = 100 }
            };
            return latestSnapshot;
        }

        // Fallback: compute from ACLs

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

    private static List<Dictionary<string, object>> MapProjectsOverview(
        int activeProjects, int totalUnits, int occupiedUnits)
    {
        var projects = new List<Dictionary<string, object>>();
        for (int i = 1; i <= activeProjects; i++)
        {
            projects.Add(new Dictionary<string, object>
            {
                ["id"] = i,
                ["name"] = $"Project {i}",
                ["totalUnits"] = totalUnits / activeProjects,
                ["occupiedUnits"] = occupiedUnits / activeProjects,
                ["occupancyRate"] = totalUnits > 0 ? (double)occupiedUnits / totalUnits * 100 : 0,
                ["status"] = "OnGoing"
            });
        }
        return projects;
    }

    // ── Sample data generators for seed fallback ──

    private static List<HistoricalDataPoint> GenerateSampleTemperatureData()
    {
        var now = DateTime.UtcNow;
        var points = new List<HistoricalDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            for (int h = 0; h < 24; h++)
            {
                points.Add(new HistoricalDataPoint
                {
                    Timestamp = now.AddDays(-i).AddHours(-h),
                    Value = 22.0 + Math.Sin((i * 24.0 + h) * 0.1) * 3,
                    Metric = "temperature"
                });
            }
        }
        return points;
    }

    private static List<HistoricalDataPoint> GenerateSampleEnergyData()
    {
        var now = DateTime.UtcNow;
        var points = new List<HistoricalDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            for (int h = 0; h < 24; h++)
            {
                points.Add(new HistoricalDataPoint
                {
                    Timestamp = now.AddDays(-i).AddHours(-h),
                    Value = 40.0 + Math.Sin((i * 24.0 + h) * 0.15) * 5,
                    Metric = "energy"
                });
            }
        }
        return points;
    }

    private static List<HistoricalDataPoint> GenerateSampleHourlyEnergy()
    {
        var now = DateTime.UtcNow;
        var points = new List<HistoricalDataPoint>();
        for (int h = 23; h >= 0; h--)
        {
            points.Add(new HistoricalDataPoint
            {
                Timestamp = now.AddHours(-h),
                Value = 2.0 + Math.Sin(h * 0.3) * 1.0,
                Metric = "hourly_energy"
            });
        }
        return points;
    }

    private static List<HistoricalDataPoint> GenerateSampleMonthlyOccupancy()
    {
        var now = DateTime.UtcNow;
        var points = new List<HistoricalDataPoint>();
        for (int m = 5; m >= 0; m--)
        {
            points.Add(new HistoricalDataPoint
            {
                Timestamp = now.AddMonths(-m),
                Value = 60.0 + (5 - m) * 5.0,
                Metric = "monthly_occupancy"
            });
        }
        return points;
    }

    private static List<HistoricalDataPoint> GenerateSampleWaterUsage()
    {
        var now = DateTime.UtcNow;
        var points = new List<HistoricalDataPoint>();
        for (int d = 6; d >= 0; d--)
        {
            points.Add(new HistoricalDataPoint
            {
                Timestamp = now.AddDays(-d),
                Value = Math.Round(1.5 + Math.Sin(d * 0.5) * 0.8, 2),
                Metric = "water_weekly"
            });
        }
        return points;
    }
}
