using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.UnitTests;

/// <summary>
/// Tests for Device.Source field (S5 — SC-5.1, SC-5.2, SC-5.3).
/// TDD: T-B4 (RED written before Device.Source exists).
/// </summary>
public class DeviceAggregateSourceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceAggregateSourceTests()
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

    // ── SC-5.1: floor-provisioned constructor sets Source = "FloorProvisioned" ──

    [Fact]
    public void Device_FloorProvisionedConstructor_SetsSource_FloorProvisioned()
    {
        var device = new Device(
            name: "Smart Meter - Floor 1",
            type: "SmartMeter",
            location: "Floor 1",
            macAddress: "F1:AA:BB:CC:DD:EE",
            projectId: 1,
            status: "Active",
            floorNumber: 1);

        device.Source.Should().Be("FloorProvisioned",
            "floor-provisioned constructor must set Source to 'FloorProvisioned' (SC-5.1)");
    }

    // ── SC-5.3: standard (manual) constructor leaves Source null ──

    [Fact]
    public void Device_StandardConstructor_Source_IsNull()
    {
        var device = new Device(
            name: "Manual Device",
            type: "SmartMeter",
            location: "Lab",
            macAddress: "00:11:22:33:44:55",
            projectId: 1,
            status: "Active");

        device.Source.Should().BeNull(
            "standard (manual) constructor must leave Source null so existing manual devices are unaffected (SC-5.3)");
    }

    // ── SC-5.2: EnsureCreated does not throw with nullable Source column ──

    [Fact]
    public void DevicesDbContext_EnsureCreated_DoesNotThrow_WithSourceField()
    {
        // This verifies that the DbContext mapping for Device.Source (nullable string)
        // does not prevent in-memory SQLite schema creation used by all Devices tests.
        var act = () => BuildContext();
        act.Should().NotThrow("EnsureCreated must succeed with nullable Source column (SC-5.2)");
    }

    // ── SC-5.2: Source column persists and reads back correctly ──

    [Fact]
    public async Task Device_Source_PersistsAndReadsBack_FromSqlite()
    {
        await using var ctx = BuildContext();

        var device = new Device(
            name: "SM Floor 1",
            type: "SmartMeter",
            location: "Floor 1",
            macAddress: "F1:00:00:00:00:01",
            projectId: 500,
            status: "Active",
            floorNumber: 1);

        await ctx.Devices.AddAsync(device);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Devices.FirstAsync(d => d.ProjectId == 500 && d.Type == "SmartMeter");
        loaded.Source.Should().Be("FloorProvisioned",
            "Source value must survive a round-trip to SQLite");
    }

    [Fact]
    public async Task Device_ManualSource_PersistsAsNull_FromSqlite()
    {
        await using var ctx = BuildContext();

        var device = new Device(
            name: "Manual SM",
            type: "SmartMeter",
            location: "Lab",
            macAddress: "00:00:00:00:AA:BB",
            projectId: 501,
            status: "Active");

        await ctx.Devices.AddAsync(device);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Devices.FirstAsync(d => d.ProjectId == 501);
        loaded.Source.Should().BeNull(
            "manually-created device with no Source must persist and read back as null");
    }
}
