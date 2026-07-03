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
/// ResolveControllable — resolves a device type's controllable attributes from the global
/// catalog (device_types table, seeded via the AddDeviceTypeCatalog migration).
///
/// Test matrix:
///   D-2a : catalog type AirConditioner resolves via DB catalog.
///   D-2c : unknown type → null → 400 downstream.
///   D-2e : out-of-range number attr rejected via catalog type → 400.
///   D-2f : valid number attr accepted via catalog type → 200.
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
    // D-2c: Catalog miss → null → caller returns 400
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveControllable_UnknownType_ReturnsNull()
    {
        await using var db = BuildInMemoryContext(nameof(ResolveControllable_UnknownType_ReturnsNull));
        var svc = BuildService(db);

        var attrs = await svc.ResolveControllable("GhostDevice", ownerId: "owner-1");

        attrs.Should().BeNull("type not in catalog → no capabilities");
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
}
