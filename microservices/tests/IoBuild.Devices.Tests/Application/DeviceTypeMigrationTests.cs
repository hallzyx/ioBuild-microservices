using FluentAssertions;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// DT-2 (TDD RED-first): DeviceType catalog seed integration tests.
/// Verifies the device_types table schema and HasData seed rows via SQLite EnsureCreated.
///
/// Approach: use EnsureCreated() (not Migrate()) because EF migrations are generated for MySQL
/// and contain MySQL-specific collation/charset annotations that SQLite does not understand.
/// EnsureCreated() creates the schema from the EF model + executes HasData seeds — this validates
/// the entity configuration (PK, unique index config, AttributesJson column) and the seed data.
///
/// Note on "7 vs 5 rows" mismatch:
///   Spec DT-2-S1 says "exactly 7 rows". Design D4 says "5 rows".
///   Tasks artifact says follow design D4: 5 rows. This test asserts 5.
///   The spec text is wrong; design D4 is authoritative.
/// </summary>
public class DeviceTypeMigrationTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceTypeMigrationTests()
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
        // Use EnsureCreated: creates schema from EF model + applies HasData seeds.
        // Avoids MySQL-specific migration DDL that SQLite does not understand.
        ctx.Database.EnsureCreated();
        return ctx;
    }

    // ── DT-2-S1 (adjusted to 5 per design D4): Fresh DB seeded with 5 rows ───

    [Fact]
    public async Task Migration_SeedsExactlyFiveDeviceTypeRows()
    {
        await using var db = BuildRelationalContext();

        var count = await db.DeviceTypes.CountAsync();

        count.Should().Be(5,
            "design D4 mandates exactly 5 catalog rows (spec says 7 but design overrides)");
    }

    // ── DT-2-S1 continued: AirConditioner has 3 attributes ───────────────────

    [Fact]
    public async Task Migration_AirConditioner_HasThreeAttributes()
    {
        await using var db = BuildRelationalContext();

        var ac = await db.DeviceTypes.SingleAsync(dt => dt.Code == "AirConditioner");
        var attrs = ac.GetAttributes();

        attrs.Should().HaveCount(3, "AirConditioner has targetTemperature + mode + power");
    }

    // ── DT-2-S1 continued: SmartLight has 2 attributes ───────────────────────

    [Fact]
    public async Task Migration_SmartLight_HasTwoAttributes()
    {
        await using var db = BuildRelationalContext();

        var sl = await db.DeviceTypes.SingleAsync(dt => dt.Code == "SmartLight");
        var attrs = sl.GetAttributes();

        attrs.Should().HaveCount(2, "SmartLight has brightness + power");
    }

    // ── DT-2-S1 continued: Floor telemetry-only types have zero attributes ───

    [Theory]
    [InlineData("SmartMeter")]
    [InlineData("WaterSensor")]
    [InlineData("SmokeDetector")]
    public async Task Migration_TelemetryOnlyTypes_HaveZeroAttributes(string code)
    {
        await using var db = BuildRelationalContext();

        var dt = await db.DeviceTypes.SingleAsync(d => d.Code == code);

        dt.GetAttributes().Should().BeEmpty(
            $"{code} is a telemetry-only type with no controllable attributes");
    }

    // ── DT-2-S3: Unique constraint on Code enforced at DB level ──────────────

    [Fact]
    public async Task Migration_UniqueIndexOnCode_PreventsDuplicates()
    {
        await using var db = BuildRelationalContext();

        // Try to insert a duplicate Code — should fail
        db.DeviceTypes.Add(new IoBuild.Devices.Domain.Model.Aggregates.DeviceType(
            "AirConditioner", "Duplicate AC", "unit", []));

        var act = async () => await db.SaveChangesAsync();

        await act.Should().ThrowAsync<Exception>(
            "unique index on Code must prevent duplicate inserts");
    }

    // ── DT-2-S2 re-seed idempotency: EnsureCreated is a no-op on existing schema ─

    [Fact]
    public async Task Migration_RerunEnsureCreated_IsIdempotent()
    {
        await using var db = BuildRelationalContext();

        // EnsureCreated again — must not duplicate seed rows
        db.Database.EnsureCreated();

        var count = await db.DeviceTypes.CountAsync();

        count.Should().Be(5, "re-running EnsureCreated must not duplicate HasData seed rows");
    }
}
