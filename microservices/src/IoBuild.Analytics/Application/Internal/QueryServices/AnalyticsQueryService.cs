using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Domain.Model.Entities;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Analytics.Application.Internal.QueryServices;

/// <remarks>
/// Metrics are eventually consistent with source services
/// (Transactional Outbox, ~5 s lag).
/// </remarks>
public class AnalyticsQueryService : IAnalyticsQueryService
{
    private readonly AnalyticsDbContext _db;
    private readonly ILogger<AnalyticsQueryService> _logger;

    // ACL facades (IDevicesContextFacade / IProjectsContextFacade) have been removed.
    // All metrics are now computed from the local projection tables populated by
    // AnalyticsEventConsumer (ADR-6, REQ-AQ-01).

    public AnalyticsQueryService(
        AnalyticsDbContext db,
        ILogger<AnalyticsQueryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Computes builder metrics from local projection tables (ADR-6, REQ-AQ-02).
    /// Returns a zeroed BuilderMetrics when tables are empty — no exceptions (ADR-7).
    /// </summary>
    public async Task<BuilderMetrics?> Handle(GetBuilderDashboardQuery query)
    {
        _logger.LogInformation("Building builder dashboard for user {UserId}", query.UserId);

        // Projects owned by this builder
        var builderProjectIds = await _db.ProjectProjections
            .Where(p => p.BuilderUserId == query.UserId)
            .Select(p => p.ProjectId)
            .ToListAsync();

        // Active projects (status = "OnGoing" or any non-null status — all are active)
        var activeProjectsCount = builderProjectIds.Count;

        // Devices whose project_id belongs to this builder (via project_projection join).
        // Uses a correlated EXISTS subquery instead of an in-memory List.Contains so the
        // expression is translatable by the MySQL provider (EF Core primitive-collection
        // translation is not enabled for this provider/server combination).
        var devices = await _db.DeviceProjections
            .Where(d => d.ProjectId != null && _db.ProjectProjections
                .Any(p => p.BuilderUserId == query.UserId && p.ProjectId == d.ProjectId!.Value))
            .ToListAsync();

        var totalDevices   = devices.Count;
        var onlineDevices  = devices.Count(d => d.Status.Equals("Online", StringComparison.OrdinalIgnoreCase));
        var offlineDevices = devices.Count(d => !d.Status.Equals("Online", StringComparison.OrdinalIgnoreCase));

        // Devices by type (for DevicesByType dictionary)
        var devicesByType = devices
            .GroupBy(d => d.DeviceType)
            .ToDictionary(g => g.Key, g => g.Count());

        // Units owned by this builder
        var units = await _db.UnitProjections
            .Where(u => u.BuilderUserId == query.UserId)
            .ToListAsync();

        var totalUnits    = units.Count;
        var occupiedUnits = units.Count(u => u.Status.Equals("Occupied", StringComparison.OrdinalIgnoreCase));
        var occupancyRate = totalUnits > 0
            ? (double)occupiedUnits / totalUnits * 100
            : 0;

        // Projects overview (name + unit rollup)
        var projects = await _db.ProjectProjections
            .Where(p => p.BuilderUserId == query.UserId)
            .ToListAsync();

        var projectsOverview = projects.Select(p =>
        {
            var pUnits    = units.Where(u => u.ProjectId == p.ProjectId).ToList();
            var pOccupied = pUnits.Count(u => u.Status.Equals("Occupied", StringComparison.OrdinalIgnoreCase));
            var pTotal    = pUnits.Count;
            return new Dictionary<string, object>
            {
                ["id"]            = p.ProjectId,
                ["name"]          = p.Name,
                ["status"]        = p.Status,
                ["totalUnits"]    = pTotal,
                ["occupiedUnits"] = pOccupied,
                ["occupancyRate"] = pTotal > 0 ? (double)pOccupied / pTotal * 100 : 0.0
            };
        }).ToList<Dictionary<string, object>>();

        return new BuilderMetrics
        {
            TotalDevices         = totalDevices,
            OnlineDevices        = onlineDevices,
            OfflineDevices       = offlineDevices,
            AlertsCount          = 0,    // No alert event in scope (was fake before — documented)
            ActiveProjectsCount  = activeProjectsCount,
            TotalUnits           = totalUnits,
            OccupiedUnits        = occupiedUnits,
            OccupancyRate        = occupancyRate,
            EnergyEfficiencyAvg  = 0,    // Telemetry out of scope (InfluxDB path removed)
            DevicesByType        = devicesByType,
            ProjectsOverview     = projectsOverview,
            // Chart time-series remain empty — telemetry out of scope (ADR-6)
            TemperatureHistory   = [],
            EnergyHistory        = [],
            HourlyEnergyData     = [],
            MonthlyOccupancy     = []
        };
    }

    /// <summary>
    /// Computes owner metrics from local projection tables (ADR-6, REQ-AQ-02, REQ-D1).
    ///
    /// Device visibility is derived via a read-side join:
    ///   DeviceProjection.UnitId == UnitProjection.UnitId WHERE UnitProjection.OwnerUserId == requestingUser
    ///
    /// DeviceProjection.OwnerUserId is always 0 (Device aggregate has no owner concept —
    /// owner identity is tracked exclusively in UnitProjection.OwnerUserId, populated by
    /// UnitOwnerMatchedEvent). Devices whose unit is not yet owner-matched are correctly
    /// excluded until the projection is updated (eventual-consistency, ADR-6).
    ///
    /// Uses a correlated EXISTS subquery (Any()) instead of in-memory List.Contains so the
    /// expression is translatable by the MySQL EF Core provider (same convention as the
    /// builder dashboard join above, lines ~51-54).
    ///
    /// Returns zeroed metrics when tables are empty — no exceptions (ADR-7).
    /// </summary>
    public async Task<OwnerMetrics?> Handle(GetOwnerDashboardQuery query)
    {
        _logger.LogInformation("Building owner dashboard for user {UserId}", query.UserId);

        // Devices whose UnitId maps to a unit owned by this user.
        // Correlated EXISTS subquery — do NOT use List.Contains (MySQL primitive-collection
        // translation limitation; same constraint documented at builder dashboard, line ~51).
        // DeviceProjection.OwnerUserId is always 0 — do NOT filter on it.
        var devices = await _db.DeviceProjections
            .Where(d => d.UnitId != null && _db.UnitProjections
                .Any(u => u.OwnerUserId == query.UserId && u.UnitId == d.UnitId!.Value))
            .ToListAsync();

        var totalDevices   = devices.Count;
        var onlineDevices  = devices.Count(d => d.Status.Equals("Online", StringComparison.OrdinalIgnoreCase));
        var offlineDevices = devices.Count(d => !d.Status.Equals("Online", StringComparison.OrdinalIgnoreCase));

        // Device health status list
        var deviceHealthStatus = devices.Select(d => new DeviceHealthStatus
        {
            DeviceId   = d.DeviceId,
            DeviceName = $"{d.DeviceType} #{d.DeviceId}",
            Status     = d.Status,
            LastOnline = d.LastEventAt
        }).ToList();

        // Units owned by this user
        var units = await _db.UnitProjections
            .Where(u => u.OwnerUserId == query.UserId)
            .ToListAsync();

        var myUnitsCount = units.Count;

        // Units details with project name.
        // Correlated EXISTS subquery instead of in-memory List.Contains (see builder
        // dashboard note) so the MySQL provider can translate the expression.
        var projectNames = await _db.ProjectProjections
            .Where(p => _db.UnitProjections
                .Any(u => u.OwnerUserId == query.UserId && u.ProjectId == p.ProjectId))
            .ToDictionaryAsync(p => p.ProjectId, p => p.Name);

        var myUnitsDetails = units.Select(u => new Dictionary<string, object>
        {
            ["unitId"]      = u.UnitId,
            ["projectName"] = projectNames.GetValueOrDefault(u.ProjectId, "Unknown"),
            ["status"]      = u.Status
        }).ToList<Dictionary<string, object>>();

        return new OwnerMetrics
        {
            TotalDevices          = totalDevices,
            OnlineDevices         = onlineDevices,
            OfflineDevices        = offlineDevices,
            AlertsCount           = 0,
            MyUnitsCount          = myUnitsCount,
            EnergyThisMonth       = 0,  // Telemetry out of scope
            TemperatureAvg        = 0,  // Telemetry out of scope
            WaterUsageThisMonth   = 0,  // Telemetry out of scope
            TemperatureHistory    = [],
            EnergyHistory         = [],
            DailyEnergyConsumption = [],
            WaterUsageWeekly      = [],
            DeviceHealthStatus    = deviceHealthStatus,
            MyUnitsDetails        = myUnitsDetails
        };
    }

    /// <summary>
    /// Returns empty — telemetry data path removed with ACL facades.
    /// // Eventually consistent — telemetry out of scope
    /// </summary>
    public Task<IEnumerable<HistoricalDataPoint>> Handle(GetHistoricalDataQuery query)
    {
        _logger.LogInformation(
            "GetHistoricalData called for project {ProjectId} — telemetry out of scope, returning empty",
            query.ProjectId);

        // Eventually consistent — telemetry out of scope
        return Task.FromResult<IEnumerable<HistoricalDataPoint>>([]);
    }
}
