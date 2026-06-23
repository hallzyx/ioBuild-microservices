using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Mqtt;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// D-2 (TDD RED-first): ResolveControllable — single resolver that tries the static catalog
/// first, then falls back to the owner's custom types in the DB.
///
/// The method is exposed as internal/public static on DeviceActuationService for testability.
/// </summary>
public class ResolveControllableTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public ResolveControllableTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private DevicesDbContext BuildSqliteContext()
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseSqlite(_connection)
            .Options;
        var ctx = new DevicesDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static DevicesDbContext BuildInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new DevicesDbContext(options);
    }

    private static DeviceActuationService BuildService(DevicesDbContext db)
    {
        var logger = new Mock<ILogger<DeviceActuationService>>().Object;
        return new DeviceActuationService(db, new Mock<IMqttPublisher>().Object, logger);
    }

    // ── D-2a: Catalog branch returns static attributes (no DB hit) ────────────

    [Fact]
    public async Task ResolveControllable_CatalogType_ReturnsStaticAttributes()
    {
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_CatalogType_ReturnsStaticAttributes));
        var svc = BuildService(db);

        var attrs = await svc.ResolveControllable("AirConditioner", ownerId: "any-owner");

        attrs.Should().NotBeNull("AirConditioner is in the static catalog");
        attrs!.Should().Contain(a => a.Name == "targetTemperature");
    }

    // ── D-2b: DB-fallback returns custom attrs when type not in catalog ────────

    [Fact]
    public async Task ResolveControllable_CustomType_ReturnsDatabaseAttributes()
    {
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_CustomType_ReturnsDatabaseAttributes));

        // Seed a custom type for owner "owner-1"
        var customType = new OwnerCustomDeviceType(
            ownerUserId: "owner-1",
            typeCode: "CO2Sensor",
            displayName: "CO2 Sensor",
            attributes: [new OwnerCustomDeviceTypeAttribute("co2_ppm", "number", 0, 5000, "ppm")]);
        db.OwnerCustomDeviceTypes.Add(customType);
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        var attrs = await svc.ResolveControllable("CO2Sensor", ownerId: "owner-1");

        attrs.Should().NotBeNull("CO2Sensor is a custom type for owner-1");
        attrs!.Should().Contain(a => a.Name == "co2_ppm");
    }

    // ── D-2c: Both-miss → returns null ────────────────────────────────────────

    [Fact]
    public async Task ResolveControllable_UnknownType_ReturnsNull()
    {
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_UnknownType_ReturnsNull));
        var svc = BuildService(db);

        var attrs = await svc.ResolveControllable("GhostDevice", ownerId: "owner-1");

        attrs.Should().BeNull("type not in catalog and not in DB → no capabilities");
    }

    // ── D-2d: Custom type belonging to different owner not returned ────────────

    [Fact]
    public async Task ResolveControllable_CustomType_WrongOwner_ReturnsNull()
    {
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_CustomType_WrongOwner_ReturnsNull));

        var customType = new OwnerCustomDeviceType(
            "owner-A", "CO2Sensor", "CO2 Sensor",
            [new OwnerCustomDeviceTypeAttribute("co2", "number", 0, 5000, "ppm")]);
        db.OwnerCustomDeviceTypes.Add(customType);
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        // owner-B asks for owner-A's type
        var attrs = await svc.ResolveControllable("CO2Sensor", ownerId: "owner-B");

        attrs.Should().BeNull("a different owner's custom type must not be returned");
    }

    // ── D-2e: Actuation: out-of-range number attr rejected → 400 ──────────────

    [Fact]
    public async Task Actuation_CustomType_OutOfRangeValue_Returns400()
    {
        await using var db = BuildInMemoryContext(nameof(Actuation_CustomType_OutOfRangeValue_Returns400));

        // Seed device (OwnerCustom) + custom type + unit ownership projection
        var customType = new OwnerCustomDeviceType(
            "42", "TempSensor", "Temp Sensor",
            [new OwnerCustomDeviceTypeAttribute("temperature", "number", 0, 100, "C")]);
        db.OwnerCustomDeviceTypes.Add(customType);

        var device = Device.ForOwnerCustom("My Temp", "TempSensor", "Unit 01",
            "OC:EE:FF:00:11:22", 1, "Active", unitId: 10);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 10, OwnerUserId = 42, UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var svc = BuildService(db);
        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id, Attribute: "temperature", Value: 150.0,
            RequestingUserId: 42, RequestingUserRole: "Owner");

        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(400, "value 150 exceeds max 100");
    }

    // ── D-2f: Actuation: valid number attr accepted → 200 ─────────────────────

    [Fact]
    public async Task Actuation_CustomType_ValidValue_Returns200()
    {
        await using var db = BuildInMemoryContext(nameof(Actuation_CustomType_ValidValue_Returns200));

        var customType = new OwnerCustomDeviceType(
            "42", "TempSensor2", "Temp Sensor 2",
            [new OwnerCustomDeviceTypeAttribute("temperature", "number", 0, 100, "C")]);
        db.OwnerCustomDeviceTypes.Add(customType);

        var device = Device.ForOwnerCustom("My Temp 2", "TempSensor2", "Unit 01",
            "OC:EE:FF:00:11:33", 1, "Active", unitId: 20);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 20, OwnerUserId = 42, UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var svc = BuildService(db);
        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id, Attribute: "temperature", Value: 42.0,
            RequestingUserId: 42, RequestingUserRole: "Owner");

        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(200, "value 42 is within [0, 100]");
    }

    // ── D-2g: Actuation: valid boolean attr accepted → 200 ────────────────────

    [Fact]
    public async Task Actuation_CustomType_BooleanAttr_Returns200()
    {
        await using var db = BuildInMemoryContext(nameof(Actuation_CustomType_BooleanAttr_Returns200));

        var customType = new OwnerCustomDeviceType(
            "42", "SmartSwitch", "Smart Switch",
            [new OwnerCustomDeviceTypeAttribute("power", "boolean")]);
        db.OwnerCustomDeviceTypes.Add(customType);

        var device = Device.ForOwnerCustom("My Switch", "SmartSwitch", "Unit 01",
            "OC:EE:FF:00:11:44", 1, "Active", unitId: 30);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 30, OwnerUserId = 42, UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var svc = BuildService(db);
        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id, Attribute: "power", Value: true,
            RequestingUserId: 42, RequestingUserRole: "Owner");

        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(200, "boolean attr has no range validation");
    }
}
