using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// B-1 + B-2 (TDD RED-first): CustomDeviceTypeCommandService + CustomDeviceTypeQueryService.
///
/// SQLite used for the duplicate (unique-index) path — EF InMemory does not enforce unique indexes.
/// EF InMemory is used for logic-only tests.
/// </summary>
public class CustomDeviceTypeServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public CustomDeviceTypeServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    // ── Helpers ───────────────────────────────────────────────────────────────

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

    private static OwnerCustomDeviceTypeAttribute NumberAttr(string name = "temperature") =>
        new(name, "number", Min: 0, Max: 100, Unit: "C");

    // ── B-1a: Valid type created → returns DTO with TypeCode ──────────────────

    [Fact]
    public async Task CreateAsync_ValidType_ReturnsDtoWithTypeCode()
    {
        await using var db = BuildInMemoryContext(nameof(CreateAsync_ValidType_ReturnsDtoWithTypeCode));
        var svc = new CustomDeviceTypeCommandService(db);

        var dto = await svc.CreateAsync(
            ownerId: "owner-1",
            typeCode: "TempSensor",
            displayName: "Temperature Sensor",
            attributes: [NumberAttr()]);

        dto.TypeCode.Should().Be("TempSensor");
        dto.DisplayName.Should().Be("Temperature Sensor");
        dto.OwnerUserId.Should().Be("owner-1");
    }

    // ── B-1b: Valid type is persisted to DB ───────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidType_PersistedToDb()
    {
        await using var db = BuildInMemoryContext(nameof(CreateAsync_ValidType_PersistedToDb));
        var svc = new CustomDeviceTypeCommandService(db);

        await svc.CreateAsync("owner-1", "HumiditySensor", "Humidity Sensor", [NumberAttr("humidity")]);

        var stored = await db.OwnerCustomDeviceTypes.FirstOrDefaultAsync();
        stored.Should().NotBeNull();
        stored!.TypeCode.Should().Be("HumiditySensor");
    }

    // ── B-1c: Duplicate TypeCode (same owner) → DbUpdateException ─────────────
    // Uses SQLite so unique index is enforced.

    [Fact]
    public async Task CreateAsync_DuplicateTypeCode_SameOwner_ThrowsDbUpdateException()
    {
        await using var db = BuildSqliteContext();
        var svc = new CustomDeviceTypeCommandService(db);

        await svc.CreateAsync("owner-1", "CO2Sensor", "CO2 Sensor", [NumberAttr("co2")]);

        // Second create — same owner + same typeCode
        var act = async () =>
            await svc.CreateAsync("owner-1", "CO2Sensor", "CO2 Sensor 2", [NumberAttr("co2")]);

        await act.Should().ThrowAsync<DbUpdateException>(
            "unique index (owner_user_id, type_code) must enforce this at the DB level");
    }

    // ── B-1d: Same TypeCode, different owner → allowed ────────────────────────

    [Fact]
    public async Task CreateAsync_SameTypeCode_DifferentOwner_Succeeds()
    {
        await using var db = BuildSqliteContext();
        var svc = new CustomDeviceTypeCommandService(db);

        await svc.CreateAsync("owner-1", "MySensor", "My Sensor", [NumberAttr()]);
        var act = async () =>
            await svc.CreateAsync("owner-2", "MySensor", "My Sensor", [NumberAttr()]);

        await act.Should().NotThrowAsync("different owner, same typeCode is allowed");
    }

    // ── B-2a: ListByOwnerAsync returns only rows for requesting owner ──────────

    [Fact]
    public async Task ListByOwnerAsync_ReturnsOnlyOwnerRows()
    {
        await using var db = BuildInMemoryContext(nameof(ListByOwnerAsync_ReturnsOnlyOwnerRows));
        var cmdSvc = new CustomDeviceTypeCommandService(db);
        var querySvc = new CustomDeviceTypeQueryService(db);

        await cmdSvc.CreateAsync("owner-1", "SensorA", "Sensor A", [NumberAttr("temp")]);
        await cmdSvc.CreateAsync("owner-1", "SensorB", "Sensor B", [NumberAttr("hum")]);
        await cmdSvc.CreateAsync("owner-2", "SensorC", "Sensor C", [NumberAttr("co2")]);

        var result = await querySvc.ListByOwnerAsync("owner-1");

        result.Should().HaveCount(2, "owner-1 has two types");
        result.Should().AllSatisfy(dto => dto.OwnerUserId.Should().Be("owner-1"));
    }

    // ── B-2b: ListByOwnerAsync returns empty list when no types defined ────────

    [Fact]
    public async Task ListByOwnerAsync_NoTypes_ReturnsEmpty()
    {
        await using var db = BuildInMemoryContext(nameof(ListByOwnerAsync_NoTypes_ReturnsEmpty));
        var querySvc = new CustomDeviceTypeQueryService(db);

        var result = await querySvc.ListByOwnerAsync("owner-99");

        result.Should().BeEmpty();
    }
}
