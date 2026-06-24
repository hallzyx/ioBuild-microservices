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

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// DT-3 (TDD RED-first): GET /api/v1/devices/types returns rows from the DB catalog.
///
/// Tests assert:
///   - Response shape: { deviceTypes: [ { code, displayName, scope, controllableAttributes } ] }
///   - Scope IS present and correct (Slice 2 surfaces it so the builder pickers can
///     filter per scope; this reverses design D5's "zero frontend change" stance)
///   - Telemetry-only types (SmartMeter, WaterSensor, SmokeDetector) have controllableAttributes: []
///   - All 5 seeded rows are returned
/// </summary>
public class GetDeviceTypesCatalogTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public GetDeviceTypesCatalogTests()
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
        // EnsureCreated: creates SQLite schema from EF model + applies HasData seeds.
        // Migrations are MySQL-specific DDL that SQLite doesn't understand.
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static DevicesController BuildController(DevicesDbContext db)
    {
        var cmdSvc = new Mock<IoBuild.Devices.Domain.Services.IDeviceCommandService>().Object;
        var deviceRepo = new DeviceRepository(db);
        var querySvc = new DeviceQueryService(deviceRepo);
        var repo = new DeviceTypeRepository(db);
        var catalogQuerySvc = new DeviceTypeCatalogQueryService(repo);
        return new DevicesController(cmdSvc, querySvc, catalogQuerySvc);
    }

    // ── DT-3-S1: Returns all catalog rows (5, not 7 per design D4) ────────────

    [Fact]
    public async Task GetDeviceTypes_ReturnsAllFiveCatalogRows()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;
        body.DeviceTypes.Should().HaveCount(5,
            "design D4 seeds exactly 5 rows; spec says 7 but design overrides");
    }

    // ── DT-3-S2: Response shape matches existing contract ─────────────────────

    [Fact]
    public async Task GetDeviceTypes_ResponseShape_MatchesContract()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;

        // Each entry must have code, displayName, scope, controllableAttributes
        foreach (var dt in body.DeviceTypes)
        {
            dt.Code.Should().NotBeNullOrWhiteSpace();
            dt.DisplayName.Should().NotBeNullOrWhiteSpace();
            dt.Scope.Should().BeOneOf("floor", "unit", "both");
            dt.ControllableAttributes.Should().NotBeNull();
        }
    }

    // ── Slice 2: Scope is surfaced per type so the builder pickers can filter ──

    [Theory]
    [InlineData("SmartMeter", "floor")]
    [InlineData("WaterSensor", "floor")]
    [InlineData("SmokeDetector", "floor")]
    [InlineData("AirConditioner", "unit")]
    [InlineData("SmartLight", "unit")]
    public async Task GetDeviceTypes_SurfacesScopePerType(string code, string expectedScope)
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;
        var entry = body.DeviceTypes.SingleOrDefault(dt => dt.Code == code);

        entry.Should().NotBeNull($"catalog must contain {code}");
        entry!.Scope.Should().Be(expectedScope);
    }

    // ── DT-3-S3: Telemetry-only types have empty controllableAttributes ────────

    [Theory]
    [InlineData("SmartMeter")]
    [InlineData("WaterSensor")]
    [InlineData("SmokeDetector")]
    public async Task GetDeviceTypes_TelemetryOnlyTypes_HaveEmptyControllableAttributes(string code)
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;
        var entry = body.DeviceTypes.SingleOrDefault(dt => dt.Code == code);

        entry.Should().NotBeNull($"catalog must contain {code}");
        entry!.ControllableAttributes.Should().BeEmpty(
            $"{code} is a telemetry-only type; controllableAttributes must be []");
    }

    // ── DT-3-S2 extra: AirConditioner has 3 controllable attributes ───────────

    [Fact]
    public async Task GetDeviceTypes_AirConditioner_HasThreeControllableAttributes()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;
        var ac = body.DeviceTypes.SingleOrDefault(dt => dt.Code == "AirConditioner");

        ac.Should().NotBeNull();
        ac!.ControllableAttributes.Should().HaveCount(3);
    }

    // ── DT-3-S2 extra: SmartLight has 2 controllable attributes ──────────────

    [Fact]
    public async Task GetDeviceTypes_SmartLight_HasTwoControllableAttributes()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;
        var sl = body.DeviceTypes.SingleOrDefault(dt => dt.Code == "SmartLight");

        sl.Should().NotBeNull();
        sl!.ControllableAttributes.Should().HaveCount(2);
    }

    // ── DT-3 no-auth: AllowAnonymous verified structurally ─────────────────────
    // Verified by inspecting the attribute on the action rather than spinning up
    // a full WebApplicationFactory (no auth config available in unit test).

    [Fact]
    public void GetDeviceTypes_Action_IsDecoratedWithAllowAnonymous()
    {
        var method = typeof(DevicesController).GetMethod(nameof(DevicesController.GetDeviceTypes));
        method.Should().NotBeNull();

        var hasAllowAnon = method!.GetCustomAttributes(true)
            .Any(a => a.GetType().Name.Contains("AllowAnonymous"));

        hasAllowAnon.Should().BeTrue("GET /types must be accessible without auth per spec DT-3");
    }
}
