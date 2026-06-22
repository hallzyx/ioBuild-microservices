using FluentAssertions;
using IoBuild.Devices.Domain.Constants;
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
/// Tests for FloorProvisioningConsumer (§4.1–4.3, REQ-FD-01..FD-04).
///
/// Uses SQLite-in-memory so the unique index on (ProjectId, FloorNumber, Type) is actually
/// enforced — EF InMemory does NOT enforce unique indexes, making idempotency tests
/// non-deterministic on InMemory (FD-S03 would pass even without the guard).
///
/// Consumer test seam: internal constructor takes a pre-wired DevicesDbContext directly,
/// mirroring the AnalyticsEventConsumer pattern (§9.3).
/// </summary>
public class FloorProvisioningConsumerTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public FloorProvisioningConsumerTests()
    {
        // Keep a single open connection for the lifetime of the test class so SQLite
        // in-memory database is not dropped between DbContext instantiations.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private DevicesDbContext BuildSqliteContext()
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseSqlite(_connection)
            .Options;
        var ctx = new DevicesDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static FloorProvisioningConsumer BuildConsumer(DevicesDbContext db)
    {
        var logger = new Mock<ILogger<FloorProvisioningConsumer>>().Object;
        return new FloorProvisioningConsumer(db, logger);
    }

    // ── FD-S01: Happy path — exactly 3 devices provisioned for a single floor ──

    [Fact]
    public async Task FD_S01_SingleFloor_Creates_Exactly3Devices_WithCorrectFloorNumber()
    {
        // Arrange
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 100,
            Floor = 2,
            UnitCount = 4,
            BuilderId = 1
        };

        // Act
        await consumer.ProvisionFloorAsync(evt);

        // Assert: exactly 3 devices created for project 100 (seed data uses projects 1-3)
        var devices = ctx.Devices.Where(d => d.ProjectId == 100).ToList();
        devices.Should().HaveCount(3, "one SmartMeter, one WaterSensor, one SmokeDetector per floor");

        // All must have FloorNumber = 2 and Location = "Floor 2"
        devices.Should().OnlyContain(d => d.FloorNumber == 2);
        devices.Should().OnlyContain(d => d.Location == "Floor 2");
        devices.Should().OnlyContain(d => d.ProjectId == 100);
        devices.Should().OnlyContain(d => d.Status == "Active");

        // One of each default type
        devices.Select(d => d.Type).Should().BeEquivalentTo(
            FloorDeviceDefaults.Defaults.Select(x => x.Type),
            "must create exactly one device per FloorDeviceDefaults entry");
    }

    [Fact]
    public async Task FD_S01_SingleFloor_Creates_Exactly3OutboxRows()
    {
        // Arrange
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent { ProjectId = 101, Floor = 2, UnitCount = 2, BuilderId = 1 };

        // Act
        await consumer.ProvisionFloorAsync(evt);

        // Assert: 3 outbox rows added (for project 101; ignore any seed-related outbox rows)
        var outbox = ctx.OutboxMessages.ToList();
        // All new rows for this test will have DeviceCreatedEvent — no other outbox rows are created by EnsureCreated
        var fd101Outbox = outbox.Where(m => m.EventType == "DeviceCreatedEvent" &&
            m.Payload.Contains("\"ProjectId\":101")).ToList();
        fd101Outbox.Should().HaveCount(3, "one DeviceCreatedEvent outbox row per provisioned device");
    }

    // ── FD-S02: Multiple floors → 9 devices total ──

    [Fact]
    public async Task FD_S02_MultipleFloors_Creates9Devices_Total()
    {
        // Arrange
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        // Act: 3 floors (1, 2, 3)
        for (int floor = 1; floor <= 3; floor++)
        {
            await consumer.ProvisionFloorAsync(new FloorStructureDefinedEvent
            {
                ProjectId = 200,
                Floor = floor,
                UnitCount = 2,
                BuilderId = 1
            });
        }

        // Assert: 9 devices total for project 200 (3 per floor)
        var devices = ctx.Devices.Where(d => d.ProjectId == 200).ToList();
        devices.Should().HaveCount(9);

        // Correct location per floor
        devices.Where(d => d.FloorNumber == 1).Should().HaveCount(3).And
            .OnlyContain(d => d.Location == "Floor 1");
        devices.Where(d => d.FloorNumber == 2).Should().HaveCount(3).And
            .OnlyContain(d => d.Location == "Floor 2");
        devices.Where(d => d.FloorNumber == 3).Should().HaveCount(3).And
            .OnlyContain(d => d.Location == "Floor 3");
    }

    // ── FD-S03: Idempotency — redelivery does NOT create duplicate devices ──

    [Fact]
    public async Task FD_S03_Redelivery_DoesNotCreateDuplicates_UniqueConstraintEnforced()
    {
        // Arrange
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent { ProjectId = 300, Floor = 1, UnitCount = 3, BuilderId = 1 };

        // Act: process the same event twice (simulating RabbitMQ redelivery)
        await consumer.ProvisionFloorAsync(evt);
        await consumer.ProvisionFloorAsync(evt); // redelivery — must be a no-op

        // Assert: still exactly 3 devices for project 300 (no duplicates)
        var devices = ctx.Devices.Where(d => d.ProjectId == 300).ToList();
        devices.Should().HaveCount(3,
            "redelivery must be idempotent — the unique (ProjectId, FloorNumber, Type) constraint " +
            "and the pre-check guard must prevent duplicate provisioning");
    }

    // ── FD-S04: Each DeviceCreatedEvent carries the correct FloorNumber ──

    [Fact]
    public async Task FD_S04_DeviceCreatedEvent_Payload_FloorNumber_EqualsEventFloor()
    {
        // Arrange
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent { ProjectId = 400, Floor = 3, UnitCount = 1, BuilderId = 1 };

        // Act
        await consumer.ProvisionFloorAsync(evt);

        // Assert: each outbox payload has FloorNumber = 3
        // Filter to only the rows we just created (project 400, ignore seed rows)
        var outboxRows = ctx.OutboxMessages
            .ToList()
            .Where(m => m.Payload.Contains("\"ProjectId\":400"))
            .ToList();
        outboxRows.Should().HaveCount(3);

        foreach (var row in outboxRows)
        {
            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(row.Payload)!;
            payload.Should().ContainKey("FloorNumber");
            var floorNumber = payload["FloorNumber"].GetInt32();
            floorNumber.Should().Be(3,
                $"DeviceCreatedEvent in outbox row {row.Id} must carry FloorNumber=3 (the triggering event's Floor)");
        }
    }

    // ── FloorDeviceDefaults constant: SmartMeter, WaterSensor, SmokeDetector ──

    [Fact]
    public void FloorDeviceDefaults_Contains_Exactly3ExpectedTypes()
    {
        FloorDeviceDefaults.Defaults.Should().HaveCount(3);
        FloorDeviceDefaults.Defaults.Select(d => d.Type).Should().BeEquivalentTo(
            new[] { "SmartMeter", "WaterSensor", "SmokeDetector" });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // T-B7 / PR2: Selective provisioning + idempotency fix + Source field
    // Spec: S4 (SC-4.1 .. SC-4.8), S5 (SC-5.1)
    // ─────────────────────────────────────────────────────────────────────────

    // ── SC-4.1: selected types only — WaterSensor absent ────────────────────

    [Fact]
    public async Task SC41_SelectedTypes_ProvisionExactly_RequestedTypes_Only()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1000,
            Floor = 2,
            UnitCount = 4,
            BuilderId = 1,
            DeviceTypes = new List<string> { "SmartMeter", "SmokeDetector" }
        };

        await consumer.ProvisionFloorAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 1000).ToList();
        devices.Should().HaveCount(2, "SC-4.1: exactly the 2 selected types must be provisioned");
        devices.Select(d => d.Type).Should().BeEquivalentTo(
            new[] { "SmartMeter", "SmokeDetector" },
            "SC-4.1: no WaterSensor should be created when not selected");
    }

    // ── SC-4.2: null DeviceTypes → 3 legacy defaults ─────────────────────────

    [Fact]
    public async Task SC42_NullDeviceTypes_Provisions3LegacyDefaults()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1001,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1,
            DeviceTypes = null   // legacy / null → 3 defaults
        };

        await consumer.ProvisionFloorAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 1001).ToList();
        devices.Should().HaveCount(3, "SC-4.2: null DeviceTypes must provision all 3 legacy defaults");
        devices.Select(d => d.Type).Should().BeEquivalentTo(
            new[] { "SmartMeter", "WaterSensor", "SmokeDetector" });
    }

    // ── SC-4.3: empty list DeviceTypes → 3 legacy defaults ───────────────────

    [Fact]
    public async Task SC43_EmptyDeviceTypes_Provisions3LegacyDefaults()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1002,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1,
            DeviceTypes = new List<string>()  // empty → treat as null → 3 defaults
        };

        await consumer.ProvisionFloorAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 1002).ToList();
        devices.Should().HaveCount(3, "SC-4.3: empty DeviceTypes must provision all 3 legacy defaults");
    }

    // ── SC-4.4: idempotency on selected types — re-delivery is a no-op ───────

    [Fact]
    public async Task SC44_Idempotency_SelectedTypes_RedeliveryDoesNotDuplicate()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1003,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1,
            DeviceTypes = new List<string> { "WaterSensor" }
        };

        await consumer.ProvisionFloorAsync(evt); // first delivery → 1 device
        await consumer.ProvisionFloorAsync(evt); // redelivery → must be no-op

        var devices = ctx.Devices.Where(d => d.ProjectId == 1003).ToList();
        devices.Should().HaveCount(1, "SC-4.4: re-delivery of a single-type selection must not duplicate");
    }

    // ── SC-4.5: idempotency on defaults (null) — re-delivery is a no-op ──────

    [Fact]
    public async Task SC45_Idempotency_DefaultTypes_RedeliveryDoesNotDuplicate()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1004,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1,
            DeviceTypes = null
        };

        await consumer.ProvisionFloorAsync(evt);
        await consumer.ProvisionFloorAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 1004).ToList();
        devices.Should().HaveCount(3, "SC-4.5: re-delivery of defaults must not duplicate devices");
    }

    // ── SC-4.6: selection WITHOUT SmartMeter does NOT false-trigger old sentinel ──

    [Fact]
    public async Task SC46_SelectionWithoutSmartMeter_DoesNotFalseTriggerSentinel()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1005,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1,
            DeviceTypes = new List<string> { "WaterSensor", "SmokeDetector" }
        };

        await consumer.ProvisionFloorAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 1005).ToList();
        devices.Should().HaveCount(2,
            "SC-4.6: a selection without SmartMeter must still provision correctly " +
            "(the old SmartMeter sentinel would have caused 0 devices to be created)");
        devices.Select(d => d.Type).Should().BeEquivalentTo(new[] { "WaterSensor", "SmokeDetector" });
    }

    // ── SC-4.7: Source field is set to "FloorProvisioned" on each provisioned device ──

    [Fact]
    public async Task SC47_ProvisionedDevices_Source_IsFloorProvisioned()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1006,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1,
            DeviceTypes = null
        };

        await consumer.ProvisionFloorAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 1006).ToList();
        devices.Should().OnlyContain(d => d.Source == "FloorProvisioned",
            "SC-4.7: every device provisioned by FloorProvisioningConsumer must have Source='FloorProvisioned'");
    }

    // ── SC-4.8: MAC uniqueness across the full catalog ───────────────────────

    [Fact]
    public async Task SC48_MacAddresses_AreUnique_AcrossFullCatalog()
    {
        await using var ctx = BuildSqliteContext();
        var consumer = BuildConsumer(ctx);

        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1007,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 1,
            DeviceTypes = null  // all 3 defaults
        };

        await consumer.ProvisionFloorAsync(evt);

        var devices = ctx.Devices.Where(d => d.ProjectId == 1007).ToList();
        var macs = devices.Select(d => d.MacAddress).ToList();
        macs.Should().OnlyHaveUniqueItems(
            "SC-4.8: MAC addresses must be distinct for each device type in the full catalog");
    }
}
