using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// T-14 (revised for ADR-7-unit-v2): Tests for DevicesDbContext composite unique index
/// based on stored computed column <c>unit_key = COALESCE(unit_id, 0)</c>.
///
/// Single composite index: (project_id, floor_number, unit_key, type) — NO filter clauses.
/// This replaces the previous filtered-index approach (ADR-7-unit v1) which MySQL silently
/// dropped (MySQL does not support partial/filtered indexes).
///
/// Semantic guarantees:
///   Floor devices : unit_id IS NULL → unit_key = 0
///     → (project_id, floor_number, 0,       type) — one per (project, floor, type)
///   Unit devices  : unit_id IS SET  → unit_key = unit_id
///     → (project_id, floor_number, unit_id, type) — one per (project, floor, unit, type)
///     Two units on the same floor with the same type are DISTINCT (different unit_key).
///
/// Uses SQLite so unique constraints are actually enforced.
/// EF InMemory does NOT enforce unique indexes — SQLite is required here.
/// SQLite IS relational and DOES support stored generated columns; the computed column
/// path is exercised in these tests (same code path as MySQL).
/// </summary>
public class DevicesDbContextFilteredIndexTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DevicesDbContextFilteredIndexTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private DevicesDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseSqlite(_connection)
            .Options;
        var ctx = new DevicesDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    // ── FI-01: Two units on the SAME floor with the SAME type — must NOT collide ──
    // Key regression test for the MySQL collision bug:
    //   unit_key(unit1) = 1, unit_key(unit2) = 2 → composite (500,1,1,"AC") vs (500,1,2,"AC") — distinct.

    [Fact]
    public async Task TwoUnits_SameFloor_SameType_BothPersistWithoutCollision()
    {
        await using var ctx = BuildContext();

        // Unit 1: AC on floor 1
        var device1 = Device.ForUnitPackage(
            name: "Air Conditioner - Unit 01",
            type: "AirConditioner",
            location: "Floor 1 - Room 01",
            mac: "U1:01:00:00:00:01",
            projectId: 500,
            status: "Active",
            floorNumber: 1,
            unitId: 1);

        // Unit 2: AC also on floor 1 but different unit
        var device2 = Device.ForUnitPackage(
            name: "Air Conditioner - Unit 02",
            type: "AirConditioner",
            location: "Floor 1 - Room 02",
            mac: "U1:01:00:00:00:02",
            projectId: 500,
            status: "Active",
            floorNumber: 1,
            unitId: 2);

        await ctx.Devices.AddAsync(device1);
        await ctx.Devices.AddAsync(device2);

        // Must NOT throw — unit_key(1) != unit_key(2) so the composite index does not collide.
        // This was the MySQL bug: with filtered indexes MySQL made the plain index
        // (project_id, floor_number, type) which DID collide for two units on the same floor.
        var act = async () => await ctx.SaveChangesAsync();
        await act.Should().NotThrowAsync(
            "two units on the same floor with the same device type must coexist — " +
            "unit_key = COALESCE(unit_id, 0) gives each unit a distinct composite key");
    }

    // ── FI-02: Same unit, same type — MUST be rejected ──
    // unit_key(10) = 10 both times → (501,1,10,"AC") duplicate → blocked.

    [Fact]
    public async Task SameUnit_SameType_Twice_ThrowsUniqueConstraintViolation()
    {
        await using var ctx = BuildContext();

        var device1 = Device.ForUnitPackage(
            name: "Air Conditioner - Unit 01",
            type: "AirConditioner",
            location: "Floor 1 - Room 01",
            mac: "U1:02:00:00:00:01",
            projectId: 501,
            status: "Active",
            floorNumber: 1,
            unitId: 10);

        var device2 = Device.ForUnitPackage(
            name: "Air Conditioner - Unit 01 DUP",
            type: "AirConditioner",
            location: "Floor 1 - Room 01",
            mac: "U1:02:00:00:00:02",  // different MAC so MAC unique won't fire first
            projectId: 501,
            status: "Active",
            floorNumber: 1,
            unitId: 10);  // SAME unit → same unit_key → composite collision

        await ctx.Devices.AddAsync(device1);
        await ctx.SaveChangesAsync();

        await ctx.Devices.AddAsync(device2);
        var act = async () => await ctx.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>(
            "inserting the same type for the same unit twice must violate the composite unique index " +
            "(project_id, floor_number, unit_key, type) — unit_key is the same for both rows");
    }

    // ── FI-03: Floor devices (UnitId=null) still work correctly ──────────────
    // unit_key = COALESCE(null, 0) = 0 → composite (502,1,0,"SmartMeter") — one row, no collision.

    [Fact]
    public async Task FloorDevices_UnitIdNull_StillPersistedCorrectly()
    {
        await using var ctx = BuildContext();

        var floorDevice = new Device(
            name: "Smart Meter - Floor 1",
            type: "SmartMeter",
            location: "Floor 1",
            macAddress: "F1:03:00:00:00:01",
            projectId: 502,
            status: "Active",
            floorNumber: 1);

        await ctx.Devices.AddAsync(floorDevice);
        var act = async () => await ctx.SaveChangesAsync();
        await act.Should().NotThrowAsync("floor devices with UnitId=null must still persist without error");

        var loaded = await ctx.Devices.FirstAsync(d => d.ProjectId == 502);
        loaded.UnitId.Should().BeNull();
        loaded.Source.Should().Be("FloorProvisioned");
    }

    // ── FI-04: Floor device idempotency still enforced ──────────────────────
    // Both rows: unit_key = 0 → composite (503,2,0,"SmartMeter") duplicate → blocked.

    [Fact]
    public async Task FloorDevices_SameFloorAndType_StillCollide()
    {
        await using var ctx = BuildContext();

        var floor1 = new Device(
            name: "Smart Meter - Floor 2",
            type: "SmartMeter",
            location: "Floor 2",
            macAddress: "F1:04:00:00:00:01",
            projectId: 503,
            status: "Active",
            floorNumber: 2);

        var floor1Dup = new Device(
            name: "Smart Meter - Floor 2 DUP",
            type: "SmartMeter",
            location: "Floor 2",
            macAddress: "F1:04:00:00:00:02",
            projectId: 503,
            status: "Active",
            floorNumber: 2);

        await ctx.Devices.AddAsync(floor1);
        await ctx.SaveChangesAsync();

        await ctx.Devices.AddAsync(floor1Dup);
        var act = async () => await ctx.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>(
            "floor devices: same (project_id, floor_number, type) both have unit_key=0 — " +
            "the composite unique index (project_id, floor_number, unit_key, type) must block this");
    }

    // ── FI-05: Unit device and floor device with same type on same floor coexist ──
    // Floor: unit_key=0 → (504,1,0,"SmartMeter"). Unit: unit_key=20 → (504,1,20,"SmartMeter"). Distinct.

    [Fact]
    public async Task UnitDevice_And_FloorDevice_SameFloor_SameType_Coexist()
    {
        await using var ctx = BuildContext();

        var floorDevice = new Device(
            name: "Smart Meter - Floor 1",
            type: "SmartMeter",
            location: "Floor 1",
            macAddress: "F1:05:00:00:00:01",
            projectId: 504,
            status: "Active",
            floorNumber: 1);

        // A unit-level device with same type — unit_key = 20, distinct from 0
        var unitDevice = Device.ForUnitPackage(
            name: "Smart Meter - Unit 01",
            type: "SmartMeter",
            location: "Floor 1 - Room 01",
            mac: "U1:05:00:00:00:01",
            projectId: 504,
            status: "Active",
            floorNumber: 1,
            unitId: 20);

        await ctx.Devices.AddAsync(floorDevice);
        await ctx.Devices.AddAsync(unitDevice);

        var act = async () => await ctx.SaveChangesAsync();
        await act.Should().NotThrowAsync(
            "a floor device (unit_key=0) and a unit device (unit_key=20) with the same type " +
            "on the same floor must coexist — the composite index (project_id,floor_number,unit_key,type) " +
            "is distinct because unit_key differs");
    }
}
