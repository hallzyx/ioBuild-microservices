using Microsoft.EntityFrameworkCore;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Configuration.Seed;
using IoBuild.Shared.Infrastructure.EFC.Extensions;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;

public class DevicesDbContext(DbContextOptions<DevicesDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceLog> DeviceLogs { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseSnakeCaseNamingConvention();

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Type).IsRequired().HasMaxLength(50);
            entity.Property(d => d.Location).IsRequired().HasMaxLength(200);
            entity.Property(d => d.MacAddress).IsRequired().HasMaxLength(17);
            entity.Property(d => d.Status).IsRequired().HasMaxLength(50);
            entity.HasIndex(d => d.MacAddress).IsUnique();
        });

        modelBuilder.Entity<DeviceLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Value).IsRequired().HasMaxLength(500);
            entity.Property(l => l.Type).IsRequired().HasMaxLength(50);
            entity.Property(l => l.Metadata).HasMaxLength(2000);
        });

        // Outbox messages (ADR-8b) — mirrors Subscriptions schema + EventId for tracing
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Payload).IsRequired().HasColumnType("longtext");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.EventId);
            entity.Property(e => e.CreatedAt);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });

        // Apply seed data AFTER entity configuration and naming conventions
        modelBuilder.ApplyDevicesSeedData();
    }
}
