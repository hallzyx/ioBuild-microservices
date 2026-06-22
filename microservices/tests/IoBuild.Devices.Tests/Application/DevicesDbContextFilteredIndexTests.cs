using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// T-14 (RED): Tests for DevicesDbContext filtered unique indexes (ADR-7-unit).
///
/// Two index domains (never overlap):
///   Floor index : (ProjectId, FloorNumber, Type) WHERE unit_id IS NULL
///   Unit index  : (ProjectId, UnitId, Type)      WHERE unit_id IS NOT NULL
///
/// Uses SQLite so unique constraints are actually enforced.
/// EF InMemory does NOT enforce unique indexes — SQLite is required here.
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
    // This is the key scenario that proves the filtered index fix works.

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

        // Must NOT throw — these are different units so (ProjectId, UnitId, Type) is distinct
        var act = async () => await ctx.SaveChangesAsync();
        await act.Should().NotThrowAsync(
            "two units on the same floor with the same device type must coexist — " +
            "they have different UnitIds so the unit index (ProjectId,UnitId,Type) does not collide");
    }

    // ── FI-02: Same unit, same type — MUST be rejected (unit unique index) ──

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
            unitId: 10);  // SAME unit

        await ctx.Devices.AddAsync(device1);
        await ctx.SaveChangesAsync();

        await ctx.Devices.AddAsync(device2);
        var act = async () => await ctx.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>(
            "inserting the same type for the same unit twice must violate (ProjectId,UnitId,Type) unique index");
    }

    // ── FI-03: Floor devices (UnitId=null) still work correctly ──────────────

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

    // ── FI-04: Floor device idempotency still enforced (same floor+type → collision) ──

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
            "floor devices: same (ProjectId, FloorNumber, Type) with UnitId=null must still collide " +
            "on the floor-filtered index (WHERE unit_id IS NULL)");
    }

    // ── FI-05: Unit device and floor device with same type on same floor coexist ──

    [Fact]
    public async Task UnitDevice_And_FloorDevice_SameFloor_SameType_Coexist()
    {
        await using var ctx = BuildContext();

        // A floor-level SmartMeter (hypothetical scenario — floor devices are SmartMeter/WaterSensor/SmokeDetector
        // but testing the index boundary is valuable regardless of real catalog)
        var floorDevice = new Device(
            name: "Smart Meter - Floor 1",
            type: "SmartMeter",
            location: "Floor 1",
            macAddress: "F1:05:00:00:00:01",
            projectId: 504,
            status: "Active",
            floorNumber: 1);

        // A unit-level device with same type — in different index domain
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
            "a floor device and a unit device with the same type on the same floor must coexist — " +
            "they live in separate index domains (unit_id IS NULL vs IS NOT NULL)");
    }
}
