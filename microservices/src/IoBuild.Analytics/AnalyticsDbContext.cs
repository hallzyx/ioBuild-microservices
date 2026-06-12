using Microsoft.EntityFrameworkCore;
using IoBuild.Shared.Infrastructure.EFC.Extensions;
using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Domain.Model.Entities;
using IoBuild.Analytics.Infrastructure.Persistence.EFC.Configuration.Seed;

namespace IoBuild.Analytics;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<BuilderMetrics> BuilderMetrics { get; init; }
    public DbSet<OwnerMetrics> OwnerMetrics { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── BuilderMetrics: store scalar metrics, ignore runtime-only complex properties ──
        modelBuilder.Entity<BuilderMetrics>(entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd();
            entity.HasKey("Id");
            entity.Property<int>("UserId");
            entity.Property<DateTime>("SnapshotDate");

            entity.Ignore(e => e.TemperatureHistory);
            entity.Ignore(e => e.EnergyHistory);
            entity.Ignore(e => e.HourlyEnergyData);
            entity.Ignore(e => e.MonthlyOccupancy);
            entity.Ignore(e => e.DevicesByType);
            entity.Ignore(e => e.ProjectsOverview);
        });

        // ── OwnerMetrics: store scalar metrics, ignore runtime-only complex properties ──
        modelBuilder.Entity<OwnerMetrics>(entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd();
            entity.HasKey("Id");
            entity.Property<int>("UserId");
            entity.Property<DateTime>("SnapshotDate");

            entity.Ignore(e => e.TemperatureHistory);
            entity.Ignore(e => e.EnergyHistory);
            entity.Ignore(e => e.DailyEnergyConsumption);
            entity.Ignore(e => e.WaterUsageWeekly);
            entity.Ignore(e => e.DeviceHealthStatus);
            entity.Ignore(e => e.MyUnitsDetails);
        });

        // Must come after entity configuration so shadow properties are known
        modelBuilder.UseSnakeCaseNamingConvention();

        // Keyless entity types for runtime-only DTOs
        modelBuilder.Entity<HistoricalDataPoint>().HasNoKey();
        modelBuilder.Entity<DeviceHealthStatus>().HasNoKey();

        // ── Seed historical data for dashboards ──
        modelBuilder.ApplyAnalyticsSeedData();
    }
}
