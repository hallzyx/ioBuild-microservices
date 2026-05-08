using Microsoft.EntityFrameworkCore;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Shared.Infrastructure.EFC.Extensions;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;

public class DevicesDbContext(DbContextOptions<DevicesDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceLog> DeviceLogs { get; set; }

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
    }
}
