using FluentAssertions;
using IoBuild.Devices.Infrastructure.Messaging;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// T-16 (RED): Tests for UnitDeviceProvisioningConsumer.
///
/// Consumer test seam: internal constructor takes a pre-wired DevicesDbContext directly,
/// mirroring FloorProvisioningConsumer test pattern.
///
/// Uses SQLite-in-memory so unique indexes are actually enforced
/// (EF InMemory does NOT enforce them).
/// </summary>
public class UnitDeviceProvisioningConsumerTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public UnitDeviceProvisioningConsumerTests()
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

    private static UnitDeviceProvisioningConsumer BuildConsumer(DevicesDbContext db)
    {
        var logger = new Mock<ILogger<UnitDeviceProvisioningConsumer>>().Object;
        return new UnitDeviceProvisioningConsumer(db, logger);
    }

    // ── UP-01: Provisions exactly the selected types, UnitId set, Source="UnitPackage" ──

    [Fact]
    public async Task UP01_ProvisionUnit_Creates_CorrectDevices_WithUnitIdAndSource()
    {
        await using var ctx = BuildContext();
        var consumer = BuildConsumer(ctx);

        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 1,
            ProjectId = 600,
            Floor = 2,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = ["AirConditioner", "SmartLight"]
        };

        await consumer.ProvisionUnitAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 600).ToList();
        devices.Should().HaveCount(2, "exactly the 2 selected types must be provisioned");
        devices.Should().OnlyContain(d => d.UnitId == 1, "all devices must carry the real UnitId");
        devices.Should().OnlyContain(d => d.Source == "UnitPackage", "Source must be 'UnitPackage'");
        devices.Should().OnlyContain(d => d.FloorNumber == 2, "FloorNumber from the event must be set");
        devices.Should().OnlyContain(d => d.ProjectId == 600);
        devices.Select(d => d.Type).Should().BeEquivalentTo(["AirConditioner", "SmartLight"]);
    }

    // ── UP-02: Idempotency — re-delivery of the same event is a no-op ──────

    [Fact]
    public async Task UP02_Redelivery_IsNoOp_DoesNotCreateDuplicates()
    {
        await using var ctx = BuildContext();
        var consumer = BuildConsumer(ctx);

        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 2,
            ProjectId = 601,
            Floor = 1,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = ["AirConditioner"]
        };

        await consumer.ProvisionUnitAsync(evt);  // first delivery
        await consumer.ProvisionUnitAsync(evt);  // redelivery — must be no-op

        var devices = ctx.Devices.Where(d => d.ProjectId == 601).ToList();
        devices.Should().HaveCount(1, "re-delivery must not create duplicate devices (idempotent)");
    }

    // ── UP-03: Empty DeviceTypes → no devices created ───────────────────────

    [Fact]
    public async Task UP03_EmptyDeviceTypes_NoDevicesCreated()
    {
        await using var ctx = BuildContext();
        var consumer = BuildConsumer(ctx);

        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 3,
            ProjectId = 602,
            Floor = 1,
            RoomNumber = "02",
            BuilderId = 1,
            DeviceTypes = []
        };

        await consumer.ProvisionUnitAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 602).ToList();
        devices.Should().BeEmpty("empty package means no devices should be provisioned");
    }

    // ── UP-04: MAC addresses are unique across the package per (project, unit) ──

    [Fact]
    public async Task UP04_MacAddresses_AreUnique_WithinPackage()
    {
        await using var ctx = BuildContext();
        var consumer = BuildConsumer(ctx);

        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 4,
            ProjectId = 603,
            Floor = 1,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = ["AirConditioner", "SmartLight"]
        };

        await consumer.ProvisionUnitAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 603).ToList();
        devices.Should().HaveCount(2);
        var macs = devices.Select(d => d.MacAddress).ToList();
        macs.Should().OnlyHaveUniqueItems("MAC addresses must be distinct within the provisioned package");
    }

    // ── UP-05: MAC prefix "U1:" distinguishes from floor MACs ("F1:") ────────

    [Fact]
    public async Task UP05_Mac_HasUnitPrefix_NotFloorPrefix()
    {
        await using var ctx = BuildContext();
        var consumer = BuildConsumer(ctx);

        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 5,
            ProjectId = 604,
            Floor = 1,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = ["AirConditioner"]
        };

        await consumer.ProvisionUnitAsync(evt);

        var device = ctx.Devices.Single(d => d.ProjectId == 604);
        device.MacAddress.Should().StartWith("U1:",
            "unit-package MACs must use 'U1:' prefix to avoid collision with floor 'F1:' MACs");
    }

    // ── UP-06: DeviceCreatedEvent outbox rows are written with UnitId populated ──

    [Fact]
    public async Task UP06_DeviceCreatedEvent_Outbox_HasUnitId_Populated()
    {
        await using var ctx = BuildContext();
        var consumer = BuildConsumer(ctx);

        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 6,
            ProjectId = 605,
            Floor = 3,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = ["SmartLight"]
        };

        await consumer.ProvisionUnitAsync(evt);

        var outboxRows = ctx.OutboxMessages
            .ToList()
            .Where(m => m.EventType == "DeviceCreatedEvent" && m.Payload.Contains("\"ProjectId\":605"))
            .ToList();

        outboxRows.Should().HaveCount(1, "one DeviceCreatedEvent outbox row per provisioned device");

        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(outboxRows[0].Payload)!;
        payload.Should().ContainKey("UnitId");
        payload["UnitId"].GetInt32().Should().Be(6, "DeviceCreatedEvent must carry the real UnitId");
        payload.Should().ContainKey("FloorNumber");
        payload["FloorNumber"].GetInt32().Should().Be(3, "DeviceCreatedEvent must carry the Floor from the event");
    }

    // ── UP-07: Two units on the same floor with the same type both provision correctly ──
    // The critical regression test: proves the filtered-index fix prevents collisions.

    [Fact]
    public async Task UP07_TwoUnits_SameFloor_SameType_BothProvisionedWithoutCollision()
    {
        await using var ctx = BuildContext();
        var consumer = BuildConsumer(ctx);

        var evt1 = new UnitDevicesDefinedEvent
        {
            UnitId = 7,
            ProjectId = 606,
            Floor = 1,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = ["AirConditioner"]
        };

        var evt2 = new UnitDevicesDefinedEvent
        {
            UnitId = 8,
            ProjectId = 606,
            Floor = 1,
            RoomNumber = "02",
            BuilderId = 1,
            DeviceTypes = ["AirConditioner"]
        };

        await consumer.ProvisionUnitAsync(evt1);
        var act = async () => await consumer.ProvisionUnitAsync(evt2);

        await act.Should().NotThrowAsync(
            "two units on the same floor with the same device type must coexist — " +
            "the (ProjectId,UnitId,Type) filtered index does not collide across different units");

        var devices = ctx.Devices.Where(d => d.ProjectId == 606).ToList();
        devices.Should().HaveCount(2, "both units must have their AC provisioned");
        devices.Select(d => d.UnitId).Should().BeEquivalentTo([7, 8]);
    }

    // ── UP-08: Floor provisioning consumer still works alongside unit consumer ──

    [Fact]
    public async Task UP08_FloorAndUnitProvisioning_Coexist_NoCollision()
    {
        await using var ctx = BuildContext();

        var floorConsumer = new FloorProvisioningConsumer(
            ctx, new Mock<ILogger<FloorProvisioningConsumer>>().Object);

        var unitConsumer = BuildConsumer(ctx);

        var floorEvt = new FloorStructureDefinedEvent
        {
            ProjectId = 607,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1
            // null DeviceTypes → 3 defaults (SmartMeter, WaterSensor, SmokeDetector)
        };

        var unitEvt = new UnitDevicesDefinedEvent
        {
            UnitId = 9,
            ProjectId = 607,
            Floor = 1,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = ["AirConditioner", "SmartLight"]
        };

        await floorConsumer.ProvisionFloorAsync(floorEvt);
        var act = async () => await unitConsumer.ProvisionUnitAsync(unitEvt);
        await act.Should().NotThrowAsync(
            "floor devices (UnitId=null) and unit devices (UnitId set) live in separate index domains");

        var allDevices = ctx.Devices.Where(d => d.ProjectId == 607).ToList();
        allDevices.Should().HaveCount(5, "3 floor devices + 2 unit devices");
        allDevices.Count(d => d.Source == "FloorProvisioned").Should().Be(3);
        allDevices.Count(d => d.Source == "UnitPackage").Should().Be(2);
    }
}
