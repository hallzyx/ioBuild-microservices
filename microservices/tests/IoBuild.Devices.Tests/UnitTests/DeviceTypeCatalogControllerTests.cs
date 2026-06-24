using FluentAssertions;
using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Devices.Interfaces.REST;
using IoBuild.Devices.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IoBuild.Devices.Tests.UnitTests;

/// <summary>
/// Tests for GET /api/v1/devices/types catalog endpoint.
/// Spec: S1.1 (SC-1.1, SC-1.2) — shape, count, stability.
///
/// Updated for device-type-catalog Slice 1 (WU-1):
///   Source is now the DB catalog (design D5) not the static in-memory catalog.
///   Tests use SQLite relational context with migration-seeded data.
///
/// Note on "Codes_MatchSharedDeviceTypeCatalogKnownTypes":
///   That test asserted the old static source. After WU-1 the source is the DB
///   catalog (5 rows seeded in migration). The test is updated to assert the same
///   5 codes come from the DB, which validates the new single source of truth.
/// </summary>
public class DeviceTypeCatalogControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceTypeCatalogControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private DevicesDbContext BuildRelationalContext()
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseSqlite(_connection)
            .Options;
        var ctx = new DevicesDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static DevicesController BuildController(DevicesDbContext db)
    {
        var cmdSvc = new Mock<IoBuild.Devices.Domain.Services.IDeviceCommandService>().Object;
        var querySvc = new DeviceQueryService(new DeviceRepository(db));
        var catalogSvc = new DeviceTypeCatalogQueryService(new DeviceTypeRepository(db));
        return new DevicesController(cmdSvc, querySvc, catalogSvc);
    }

    // ── SC-1.1: happy path — 200 with exactly 5 entries, each having code + displayName ──

    [Fact]
    public async Task GetDeviceTypes_Returns200_WithFiveEntries()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();

        var result = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        result.StatusCode.Should().Be(200);

        var body = result.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;
        body.DeviceTypes.Should().HaveCount(5,
            "catalog must have 5 types: 3 floor-level + 2 unit-level (AirConditioner, SmartLight)");
    }

    [Fact]
    public async Task GetDeviceTypes_EachEntry_HasCodeAndDisplayName()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();
        var result = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = result.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;

        body.DeviceTypes.Should().OnlyContain(
            t => !string.IsNullOrWhiteSpace(t.Code) && !string.IsNullOrWhiteSpace(t.DisplayName),
            "every device type entry must have a non-empty code and displayName (SC-1.1)");
    }

    [Fact]
    public async Task GetDeviceTypes_ContainsAllFiveTypes()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();
        var result = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = result.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;

        var codes = body.DeviceTypes.Select(t => t.Code).ToList();
        codes.Should().Contain("SmartMeter");
        codes.Should().Contain("WaterSensor");
        codes.Should().Contain("SmokeDetector");
        codes.Should().Contain("AirConditioner");
        codes.Should().Contain("SmartLight");
    }

    // ── SC-1.2: stability — two calls return same order ──

    [Fact]
    public async Task GetDeviceTypes_CalledTwice_ReturnsSameOrder()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var first = (((await controller.GetDeviceTypes()) as OkObjectResult)!.Value as DeviceTypeCatalogResource)!
            .DeviceTypes.Select(t => t.Code).ToList();
        var second = (((await controller.GetDeviceTypes()) as OkObjectResult)!.Value as DeviceTypeCatalogResource)!
            .DeviceTypes.Select(t => t.Code).ToList();

        first.Should().ContainInOrder(second,
            "catalog order must be deterministic across calls (SC-1.2)");
    }

    // ── Catalog single-source: DB catalog is now authoritative (replaces static KnownTypes) ──
    // After WU-1 the source is the DB (design D5); the 5 seeded codes must match
    // the 5 known-type codes that were previously served from the static catalog.

    [Fact]
    public async Task GetDeviceTypes_Codes_MatchExpectedFiveCatalogCodes()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();
        var result = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = result.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;

        body.DeviceTypes.Select(t => t.Code).Should().BeEquivalentTo(
            new[] { "AirConditioner", "SmartLight", "SmartMeter", "SmokeDetector", "WaterSensor" },
            "DB catalog must contain the same 5 codes as the previous static source");
    }
}
