using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Exceptions;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// MAC-collision regression tests (TDD RED-first).
///
/// Root cause: IX_devices_mac_address is UNIQUE on mac_address alone.
/// The frontend sends macAddress='' for every owner-custom device.
/// First device occupies mac=''; second device collides → MySqlException 1062.
///
/// Fix: ForOwnerCustom stores MacAddress = NULL. MySQL UNIQUE indexes allow
/// multiple NULLs, so the collision disappears while real device MACs stay unique.
///
/// The MacAddress field in CreateDeviceCommand becomes nullable (string?) so the
/// backend can accept null/omitted values from the frontend without mapping '' to the DB.
///
/// Uses SQLite (relational) so the UNIQUE index on mac_address is enforced — tests
/// would be meaningless on InMemory which skips schema constraints.
/// </summary>
public class DeviceOwnerCustomMacCollisionTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceOwnerCustomMacCollisionTests()
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

    private DeviceCommandService BuildService(DevicesDbContext db) =>
        new(new DeviceRepository(db), new OutboxMessageRepository(db), db);

    private static void SeedOwnerAndTypes(DevicesDbContext db, int unitId, int ownerId,
        params string[] typeCodes)
    {
        db.UnitOwnerProjections.Add(new UnitOwnerProjection
            { UnitId = unitId, OwnerUserId = ownerId, UpdatedAt = DateTime.UtcNow });

        foreach (var tc in typeCodes)
        {
            db.OwnerCustomDeviceTypes.Add(new OwnerCustomDeviceType(
                ownerId.ToString(), tc, $"{tc} Display",
                [new OwnerCustomDeviceTypeAttribute("attr1", "number", 0, 100, "units")]));
        }
    }

    // ── COLLISION-1 (core regression): two DIFFERENT-type devices in same unit both succeed ──
    //
    // Before fix: both calls send macAddress=null (empty string from frontend maps to null).
    // DeviceCommandService calls ForOwnerCustom which stores MacAddress=''. First device
    // occupies mac=''; second device gets MySqlException 1062 "Duplicate entry '' for key
    // 'devices.IX_devices_mac_address'".
    //
    // After fix: ForOwnerCustom stores MacAddress=NULL. MySQL allows multiple NULLs in a
    // UNIQUE index — no collision.

    [Fact]
    public async Task CreateTwoDifferentTypeOwnerCustomDevices_InSameUnit_BothSucceed()
    {
        await using var db = BuildContext();
        SeedOwnerAndTypes(db, unitId: 20, ownerId: 1, "CO2Sensor", "TempSensor");
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        // First device — type CO2Sensor
        var first = await svc.Handle(new CreateDeviceCommand(
            "CO2 Monitor", "CO2Sensor", "Unit 20",
            null,  // owner-custom devices have no hardware MAC — backend stores NULL
            1, "Active",
            UnitId: 20, RequestingOwnerId: "1"));

        first.Should().NotBeNull("first owner-custom device must be created successfully");
        first.MacAddress.Should().BeNull("owner-custom devices store null mac");

        // Second device — DIFFERENT type (TempSensor) in SAME unit
        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "Temperature Monitor", "TempSensor", "Unit 20",
            null,
            1, "Active",
            UnitId: 20, RequestingOwnerId: "1"));

        await act.Should().NotThrowAsync(
            "two DIFFERENT types in the same unit must both be allowed — " +
            "the mac collision bug was blocking this");
    }

    // ── COLLISION-2: same type on same unit is still blocked via pre-check 409 ──

    [Fact]
    public async Task CreateTwoSameTypeOwnerCustomDevices_InSameUnit_SecondThrows409()
    {
        await using var db = BuildContext();
        SeedOwnerAndTypes(db, unitId: 21, ownerId: 1, "HumidSensor");
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        await svc.Handle(new CreateDeviceCommand(
            "Humidity A", "HumidSensor", "Unit 21",
            null, 1, "Active",
            UnitId: 21, RequestingOwnerId: "1"));

        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "Humidity B", "HumidSensor", "Unit 21",
            null, 1, "Active",
            UnitId: 21, RequestingOwnerId: "1"));

        await act.Should().ThrowAsync<DuplicateDeviceTypeOnUnitException>(
            "same type in same unit must still be rejected via the pre-check");
    }

    // ── COLLISION-3: real device with explicit MAC still enforces MAC uniqueness ──

    [Fact]
    public async Task CreateTwoRegularDevices_WithSameMac_SecondFails()
    {
        await using var db = BuildContext();
        await db.SaveChangesAsync();

        var svc = BuildService(db);
        const string mac = "AA:BB:CC:DD:EE:01";

        await svc.Handle(new CreateDeviceCommand(
            "Real Device 1", "Thermostat", "Floor 1", mac, 1, "Active"));

        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "Real Device 2", "Thermostat", "Floor 2", mac, 1, "Active"));

        await act.Should().ThrowAsync<Exception>(
            "two real devices with the same MAC must collide on IX_devices_mac_address");
    }
}
