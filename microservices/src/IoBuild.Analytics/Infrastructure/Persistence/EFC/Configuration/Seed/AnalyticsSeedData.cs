using Microsoft.EntityFrameworkCore;
using IoBuild.Analytics.Domain.Model.Aggregates;

namespace IoBuild.Analytics.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class AnalyticsSeedData
{
    public static void ApplyAnalyticsSeedData(this ModelBuilder builder)
    {
        var now = DateTime.UtcNow;

        // ══════════════════════════════════════════════════════════════
        // BuilderMetrics — historical snapshots for builder user (Id: 1)
        // ══════════════════════════════════════════════════════════════
        builder.Entity<BuilderMetrics>().HasData(
            // 30 days ago — starting operations, lower occupancy
            new
            {
                Id = 1,
                UserId = 1,
                SnapshotDate = now.AddDays(-30),
                TotalDevices = 35,
                OnlineDevices = 30,
                OfflineDevices = 5,
                AlertsCount = 12,
                ActiveProjectsCount = 3,
                TotalUnits = 180,
                OccupiedUnits = 120,
                OccupancyRate = 0.67,
                EnergyEfficiencyAvg = 2.8
            },
            // 20 days ago — new project added, occupancy climbing
            new
            {
                Id = 2,
                UserId = 1,
                SnapshotDate = now.AddDays(-20),
                TotalDevices = 38,
                OnlineDevices = 34,
                OfflineDevices = 4,
                AlertsCount = 9,
                ActiveProjectsCount = 4,
                TotalUnits = 200,
                OccupiedUnits = 145,
                OccupancyRate = 0.72,
                EnergyEfficiencyAvg = 3.0
            },
            // 12 days ago — more devices deployed
            new
            {
                Id = 3,
                UserId = 1,
                SnapshotDate = now.AddDays(-12),
                TotalDevices = 40,
                OnlineDevices = 36,
                OfflineDevices = 4,
                AlertsCount = 8,
                ActiveProjectsCount = 4,
                TotalUnits = 220,
                OccupiedUnits = 168,
                OccupancyRate = 0.76,
                EnergyEfficiencyAvg = 3.2
            },
            // 5 days ago — near full capacity
            new
            {
                Id = 4,
                UserId = 1,
                SnapshotDate = now.AddDays(-5),
                TotalDevices = 42,
                OnlineDevices = 38,
                OfflineDevices = 4,
                AlertsCount = 7,
                ActiveProjectsCount = 5,
                TotalUnits = 240,
                OccupiedUnits = 182,
                OccupancyRate = 0.76,
                EnergyEfficiencyAvg = 3.3
            },
            // Today — current state
            new
            {
                Id = 5,
                UserId = 1,
                SnapshotDate = now,
                TotalDevices = 42,
                OnlineDevices = 38,
                OfflineDevices = 4,
                AlertsCount = 7,
                ActiveProjectsCount = 5,
                TotalUnits = 248,
                OccupiedUnits = 186,
                OccupancyRate = 0.75,
                EnergyEfficiencyAvg = 3.4
            }
        );

        // ══════════════════════════════════════════════════════════════
        // OwnerMetrics — historical snapshots for owner user (Id: 2)
        // ══════════════════════════════════════════════════════════════
        builder.Entity<OwnerMetrics>().HasData(
            // 28 days ago — early period, higher water usage
            new
            {
                Id = 1,
                UserId = 2,
                SnapshotDate = now.AddDays(-28),
                TotalDevices = 6,
                OnlineDevices = 5,
                OfflineDevices = 1,
                AlertsCount = 3,
                MyUnitsCount = 2,
                EnergyThisMonth = 320.0,
                TemperatureAvg = 21.5,
                WaterUsageThisMonth = 14.2
            },
            // 18 days ago — slight increase in energy
            new
            {
                Id = 2,
                UserId = 2,
                SnapshotDate = now.AddDays(-18),
                TotalDevices = 7,
                OnlineDevices = 6,
                OfflineDevices = 1,
                AlertsCount = 2,
                MyUnitsCount = 2,
                EnergyThisMonth = 340.2,
                TemperatureAvg = 22.0,
                WaterUsageThisMonth = 13.5
            },
            // 8 days ago — added one more device
            new
            {
                Id = 3,
                UserId = 2,
                SnapshotDate = now.AddDays(-8),
                TotalDevices = 8,
                OnlineDevices = 7,
                OfflineDevices = 1,
                AlertsCount = 2,
                MyUnitsCount = 2,
                EnergyThisMonth = 365.8,
                TemperatureAvg = 22.5,
                WaterUsageThisMonth = 12.8
            },
            // Today — current state, energy dropping due to season
            new
            {
                Id = 4,
                UserId = 2,
                SnapshotDate = now,
                TotalDevices = 8,
                OnlineDevices = 7,
                OfflineDevices = 1,
                AlertsCount = 2,
                MyUnitsCount = 2,
                EnergyThisMonth = 285.5,
                TemperatureAvg = 22.3,
                WaterUsageThisMonth = 12.7
            }
        );
    }
}
