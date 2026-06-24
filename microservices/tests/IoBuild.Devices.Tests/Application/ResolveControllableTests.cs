using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Devices.Infrastructure.Mqtt;
using IoBuild.Shared.Domain.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// DT-4 (TDD RED-first / WU-2): ResolveControllable — catalog-first (DB device_types),
/// then owner-custom DB fallback. Static DeviceCapabilityCatalog.ByType branch removed.
///
/// Lookup order:
///   1. device_types catalog (DB, via IDeviceTypeRepository)  ← NEW (WU-2)
///   2. owner_custom_device_types (DB, per-owner fallback)    ← KEPT (D3 safety)
///   3. Both miss → null → caller returns 400.
///
/// Test matrix:
///   D-2a (updated) : catalog type AirConditioner resolves via DB catalog (not static branch).
///   D-2b (regression) : owner-custom type NOT in device_types resolves via owner-custom fallback.
///   D-2c : unknown type → null → 400 downstream.
///   D-2d : custom type belonging to different owner → null (isolation guard).
///   D-2e : out-of-range number attr rejected via catalog type → 400.
///   D-2f : valid number attr accepted via catalog type → 200.
///   D-2g : valid boolean attr accepted via owner-custom type → 200.
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

    // ── Context builders ─────────────────────────────────────────────────────

    /// <summary>
    /// SQLite relational context sharing the same in-memory connection across
    /// calls within the same test (schema/seed persists on the shared connection).
    /// </summary>
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

    // ── Service builder — accepts repository via DI ───────────────────────────

    private static DeviceActuationService BuildService(DevicesDbContext db)
    {
        var logger = new Mock<ILogger<DeviceActuationService>>().Object;
        IDeviceTypeRepository repo = new DeviceTypeRepository(db);
        return new DeviceActuationService(db, new Mock<IMqttPublisher>().Object, logger, repo);
    }

    // ── Catalog seed helper ────────────────────────────────────────────────────

    /// <summary>
    /// Seeds an AirConditioner catalog entry into the given DbContext (WU-2 new path).
    /// Matches the same attributes as DeviceCapabilityCatalog.ByType["AirConditioner"].
    /// </summary>
    private static async Task SeedAirConditionerCatalogEntry(DevicesDbContext db)
    {
        var acAttrs = new List<DeviceCapabilityCatalog.ControllableAttribute>
        {
            new("targetTemperature", "number", 16, 30, "C"),
            new("mode", "enum", null, null, null, ["cooling", "heating", "fan"]),
            new("power", "boolean", null, null, null),
        };
        var acType = new DeviceType("AirConditioner", "Air Conditioner", "unit", acAttrs);
        db.DeviceTypes.Add(acType);
        await db.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D-2a: DB catalog hit → returns attributes (static branch removed)
    // DT-4-S1: A type present in device_types resolves its attributes via the catalog.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveControllable_CatalogType_ReturnsDbCatalogAttributes()
    {
        // Arrange — seed device_types with AirConditioner using InMemory (no relational schema needed)
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_CatalogType_ReturnsDbCatalogAttributes));
        await SeedAirConditionerCatalogEntry(db);

        var svc = BuildService(db);

        // Act
        var attrs = await svc.ResolveControllable("AirConditioner", ownerId: "any-owner");

        // Assert
        attrs.Should().NotBeNull("AirConditioner is in the DB catalog");
        attrs!.Should().Contain(a => a.Name == "targetTemperature",
            "catalog entry must expose targetTemperature");
        attrs.Should().Contain(a => a.Name == "power",
            "catalog entry must expose power");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D-2b (regression guard): owner-custom type NOT in device_types catalog
    // still resolves via owner-custom DB fallback (DESIGN D3 — MUST pass).
    // DT-4: owner-custom-fallback must survive the static-branch removal.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveControllable_OwnerCustomType_NotInCatalog_ResolvesViaFallback()
    {
        // Arrange — device_types catalog intentionally empty; owner-custom type seeded
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_OwnerCustomType_NotInCatalog_ResolvesViaFallback));

        var customType = new OwnerCustomDeviceType(
            ownerUserId: "owner-1",
            typeCode: "CO2Sensor",
            displayName: "CO2 Sensor",
            attributes: [new OwnerCustomDeviceTypeAttribute("co2_ppm", "number", 0, 5000, "ppm")]);
        db.OwnerCustomDeviceTypes.Add(customType);
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        // Act
        var attrs = await svc.ResolveControllable("CO2Sensor", ownerId: "owner-1");

        // Assert — owner-custom fallback must still work (D3 regression guard)
        attrs.Should().NotBeNull("CO2Sensor is a custom type for owner-1; fallback must resolve it");
        attrs!.Should().Contain(a => a.Name == "co2_ppm",
            "owner-custom attribute co2_ppm must be returned via fallback");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D-2c: Both catalog and owner-custom miss → null → caller returns 400
    // DT-4-S2: unknown type → null.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveControllable_UnknownType_ReturnsNull()
    {
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_UnknownType_ReturnsNull));
        var svc = BuildService(db);

        var attrs = await svc.ResolveControllable("GhostDevice", ownerId: "owner-1");

        attrs.Should().BeNull("type not in catalog and not in DB → no capabilities");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D-2d: Owner isolation — custom type of another owner must not be returned
    // ─────────────────────────────────────────────────────────────────────────

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

        var attrs = await svc.ResolveControllable("CO2Sensor", ownerId: "owner-B");

        attrs.Should().BeNull("a different owner's custom type must not be returned");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D-2e: Range validation — out-of-range number attr on CATALOG type → 400
    // DT-4-S3: value outside [Min, Max] rejected.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Actuation_CatalogType_OutOfRangeValue_Returns400()
    {
        await using var db = BuildInMemoryContext(nameof(Actuation_CatalogType_OutOfRangeValue_Returns400));

        // Seed catalog type + device + ownership projection
        await SeedAirConditionerCatalogEntry(db);

        var device = Device.ForUnitPackage(
            name: "AC Test", type: "AirConditioner", location: "Floor 1",
            mac: "AA:BB:CC:DD:EE:01", projectId: 1, status: "Active",
            floorNumber: 1, unitId: 10);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 10, OwnerUserId = 42, UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = BuildService(db);
        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id, Attribute: "targetTemperature", Value: 99.0,
            RequestingUserId: 42, RequestingUserRole: "Owner");

        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(400, "value 99 exceeds max 30 for targetTemperature");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D-2f: Range validation — valid number attr on CATALOG type → 200
    // DT-4-S3: value within [Min, Max] accepted.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Actuation_CatalogType_ValidValue_Returns200()
    {
        await using var db = BuildInMemoryContext(nameof(Actuation_CatalogType_ValidValue_Returns200));

        await SeedAirConditionerCatalogEntry(db);

        var device = Device.ForUnitPackage(
            name: "AC Test 2", type: "AirConditioner", location: "Floor 1",
            mac: "AA:BB:CC:DD:EE:02", projectId: 1, status: "Active",
            floorNumber: 1, unitId: 20);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 20, OwnerUserId = 42, UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = BuildService(db);
        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id, Attribute: "targetTemperature", Value: 22.0,
            RequestingUserId: 42, RequestingUserRole: "Owner");

        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(200, "value 22 is within [16, 30]");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D-2g: Boolean attr accepted (no range validation) via OWNER-CUSTOM type → 200
    // Keeps verifying the owner-custom path end-to-end through Handle().
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Actuation_OwnerCustomType_BooleanAttr_Returns200()
    {
        await using var db = BuildInMemoryContext(nameof(Actuation_OwnerCustomType_BooleanAttr_Returns200));

        var customType = new OwnerCustomDeviceType(
            "42", "SmartSwitch", "Smart Switch",
            [new OwnerCustomDeviceTypeAttribute("power", "boolean")]);
        db.OwnerCustomDeviceTypes.Add(customType);

        var device = Device.ForOwnerCustom("My Switch", "SmartSwitch", "Unit 01",
            1, "Active", unitId: 30);
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

        result.StatusCode.Should().Be(200, "boolean attr has no range validation; owner-custom fallback must resolve");
    }
}
