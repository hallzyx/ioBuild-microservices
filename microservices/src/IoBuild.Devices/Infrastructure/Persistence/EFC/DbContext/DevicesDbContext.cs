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
            // Floor placement (PR 6, §4.4) — nullable columns
            entity.Property(d => d.FloorNumber);   // nullable int
            entity.Property(d => d.UnitId);         // nullable int (future per-unit placement)
            // Source discriminator (S5.1) — nullable, max 30 chars.
            // Nullable so existing rows and EnsureCreated InMemory test paths stay valid (S5.2).
            entity.Property(d => d.Source).HasMaxLength(30);
            entity.HasIndex(d => d.MacAddress).IsUnique();

            // ── ADR-7-unit: Two filtered unique index domains (T-14/T-15) ────────────────
            //
            // PROBLEM: the original unfiltered (ProjectId, FloorNumber, Type) index collides
            // when two units on the same floor both have an AirConditioner, because both set
            // FloorNumber. The fix: split into two filtered domains that never overlap.
            //
            // Floor domain  : (ProjectId, FloorNumber, Type) WHERE unit_id IS NULL
            //   → covers FloorProvisioningConsumer devices (UnitId never set there)
            // Unit domain   : (ProjectId, UnitId, Type)      WHERE unit_id IS NOT NULL
            //   → covers UnitDeviceProvisioningConsumer devices (UnitId always set)
            //
            // PROVIDER GUARD: EF InMemory does not honor HasFilter; wrap relational-only
            // config so EnsureCreated / InMemory test paths survive without modification.
            // SQLite (used in integration tests) DOES enforce filtered indexes — tests
            // that validate uniqueness use SQLite, not InMemory.
            if (Database.IsRelational())
            {
                // Floor index — only for rows WITHOUT a unit link
                entity.HasIndex(d => new { d.ProjectId, d.FloorNumber, d.Type })
                      .IsUnique()
                      .HasFilter("unit_id IS NULL");

                // Unit index — only for rows WITH a unit link (prevents same type twice per unit)
                entity.HasIndex(d => new { d.ProjectId, d.UnitId, d.Type })
                      .IsUnique()
                      .HasFilter("unit_id IS NOT NULL");
            }
            else
            {
                // InMemory / EnsureCreated test path: apply unfiltered indexes.
                // NULL-distinctness keeps them valid since test rows rarely collide across domains.
                entity.HasIndex(d => new { d.ProjectId, d.FloorNumber, d.Type })
                      .IsUnique();
                entity.HasIndex(d => new { d.ProjectId, d.UnitId, d.Type })
                      .IsUnique();
            }
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
