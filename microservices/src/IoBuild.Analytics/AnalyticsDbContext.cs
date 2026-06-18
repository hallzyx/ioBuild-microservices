using Microsoft.EntityFrameworkCore;
using IoBuild.Shared.Infrastructure.EFC.Extensions;
using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Domain.Model.Entities;
using IoBuild.Analytics.Domain.Model.Projections;

namespace IoBuild.Analytics;

/// <remarks>
/// Metrics are eventually consistent with source services
/// (Transactional Outbox, ~5 s lag).
/// </remarks>
public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    // ── Projection tables (ADR-6) — populated by AnalyticsEventConsumer ──
    public DbSet<DeviceProjection> DeviceProjections { get; init; }
    public DbSet<ProjectProjection> ProjectProjections { get; init; }
    public DbSet<UnitProjection> UnitProjections { get; init; }

    // ── Response DTO types (retained for REST assemblers) ──
    // BuilderMetrics and OwnerMetrics are NO longer EF-mapped tables.
    // They are computed in-process by AnalyticsQueryService.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── DeviceProjection: device_projection table ──
        modelBuilder.Entity<DeviceProjection>(entity =>
        {
            entity.HasKey(e => e.DeviceId);
            entity.Property(e => e.DeviceType).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(e => e.OwnerUserId);
            entity.HasIndex(e => e.ProjectId);
        });

        // ── ProjectProjection: project_projection table ──
        modelBuilder.Entity<ProjectProjection>(entity =>
        {
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.Name).HasMaxLength(160).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(e => e.BuilderUserId);
        });

        // ── UnitProjection: unit_projection table ──
        modelBuilder.Entity<UnitProjection>(entity =>
        {
            entity.HasKey(e => e.UnitId);
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(e => e.BuilderUserId);
            entity.HasIndex(e => e.OwnerUserId);
            entity.HasIndex(e => e.ProjectId);
        });

        // ── Keyless runtime-only DTOs ──
        modelBuilder.Entity<HistoricalDataPoint>().HasNoKey();
        modelBuilder.Entity<DeviceHealthStatus>().HasNoKey();

        // snake_case naming — must come after all entity config
        modelBuilder.UseSnakeCaseNamingConvention();
    }
}
