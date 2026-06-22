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
    public DbSet<DeviceShadow> DeviceShadows { get; set; }
    public DbSet<UnitOwnerProjection> UnitOwnerProjections { get; set; }

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

            // ── ADR-7-unit-v2: Computed column + composite unique index (MySQL-compatible) ──
            //
            // PROBLEM with v1 (filtered indexes): MySQL does NOT support partial/filtered indexes.
            // Pomelo silently drops the HasFilter() clause, generating a plain unique index on
            // (project_id, floor_number, type). Two units on the same floor with the same device
            // type both set FloorNumber, so MySQL raised ERROR 1062 (duplicate entry) even though
            // they have different UnitIds. The SQLite/InMemory tests passed because SQLite DOES
            // honour filtered indexes — the divergence was invisible until docker e2e on MySQL.
            //
            // FIX: stored computed column  unit_key = COALESCE(unit_id, 0)
            //   → Floor devices (unit_id IS NULL): unit_key = 0
            //   → Unit  devices (unit_id IS SET ): unit_key = unit_id
            //
            // Single composite unique index: (project_id, floor_number, unit_key, type)
            //   Floor: (project, floor, 0,       type) — one per (project, floor, type) ✓
            //   Unit : (project, floor, unit_id, type) — one per (project, floor, unit, type) ✓
            //          two units same floor + same type → distinct unit_key → NO collision ✓
            //          same unit, same type twice       → same unit_key     → BLOCKED ✓
            //
            // PROVIDER GUARD: EF InMemory does not support computed columns (HasComputedColumnSql).
            // Relational path (SQLite + MySQL): computed column + composite index.
            // InMemory path: fallback unfiltered indexes that keep InMemory/EnsureCreated tests valid.
            // NOTE: SQLite IS relational, so it exercises the real computed-column + composite-index
            // path — no more SQLite/MySQL divergence.
            if (Database.IsRelational())
            {
                // Stored computed column: unit_key = COALESCE(unit_id, 0)
                // 'stored: true' → persisted/generated column (supported by SQLite and MySQL).
                entity.Property<int>("UnitKey")
                      .HasColumnName("unit_key")
                      .HasComputedColumnSql("COALESCE(unit_id, 0)", stored: true);

                // Single composite unique index — no filter clause, MySQL-valid.
                entity.HasIndex("ProjectId", "FloorNumber", "UnitKey", "Type")
                      .IsUnique()
                      .HasDatabaseName("IX_devices_project_id_floor_number_unit_key_type");
            }
            else
            {
                // InMemory / EnsureCreated test path: plain unfiltered indexes.
                // These keep InMemory-backed unit tests alive without any schema changes.
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

        // ── DeviceShadow (ADR-B3): plain PK, no filtered indexes, LONGTEXT columns ──
        // W-1 fix: DeviceId is a NATURAL key (FK to Devices), NOT a surrogate/auto-increment.
        // ValueGeneratedNever() removes the IdentityColumn annotation from the migration so
        // MySQL does not auto-increment it. The FK constraint to devices enforces referential
        // integrity — orphaned shadow rows are impossible.
        modelBuilder.Entity<DeviceShadow>(entity =>
        {
            entity.HasKey(s => s.DeviceId);
            entity.Property(s => s.DeviceId).ValueGeneratedNever();
            entity.Property(s => s.DesiredJson).IsRequired().HasColumnType("longtext");
            entity.Property(s => s.ReportedJson).HasColumnType("longtext");
            entity.Property(s => s.UpdatedByUserId).IsRequired();
            entity.Property(s => s.UpdatedAt).IsRequired();
            // FK to Devices.Id — referential integrity prevents orphaned shadow rows.
            entity.HasOne<Device>().WithMany()
                  .HasForeignKey(s => s.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UnitOwnerProjection (ADR-B4): plain PK, plain unique constraint, no filtered indexes ──
        // UnitId is a natural key from Projects service — NOT auto-generated.
        modelBuilder.Entity<UnitOwnerProjection>(entity =>
        {
            entity.HasKey(p => p.UnitId);
            entity.Property(p => p.UnitId).ValueGeneratedNever();
            entity.Property(p => p.OwnerUserId).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();
            entity.HasIndex(p => new { p.UnitId, p.OwnerUserId })
                  .IsUnique()
                  .HasDatabaseName("IX_unit_owner_projections_unit_id_owner_user_id");
        });

        // Apply seed data AFTER entity configuration and naming conventions
        modelBuilder.ApplyDevicesSeedData();
    }
}
