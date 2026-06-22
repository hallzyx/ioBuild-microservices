using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Infrastructure.Mqtt;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// TDD RED tests for tasks 2.1–2.6 — DeviceActuationService permission matrix and catalog validation.
///
/// All tests use EF InMemory (no unique-index enforcement needed here — we test service logic only).
/// The fake IMqttPublisher records calls so we can assert publish happened.
/// </summary>
public class DeviceActuationServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static DevicesDbContext BuildInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new DevicesDbContext(options);
    }

    private static Device BuildAcDevice(int projectId = 1, int unitId = 10)
    {
        return Device.ForUnitPackage(
            name: "Air Conditioner - Unit 01",
            type: "AirConditioner",
            location: "Floor 1 - Room 01",
            mac: "U1:AA:BB:CC:DD:01",
            projectId: projectId,
            status: "Active",
            floorNumber: 1,
            unitId: unitId);
    }

    private static IDeviceActuationService BuildService(
        DevicesDbContext db,
        IMqttPublisher? mqttPublisher = null)
    {
        var logger = new Mock<ILogger<DeviceActuationService>>().Object;
        return new DeviceActuationService(db, mqttPublisher ?? new Mock<IMqttPublisher>().Object, logger);
    }

    // ── 2.1: Owner not in projection → 403 ────────────────────────────────────

    /// <summary>
    /// Task 2.1 RED: No projection row for requesting user → Forbidden.
    /// </summary>
    [Fact]
    public async Task DeviceActuationService_OwnerNotInProjection_Returns403()
    {
        await using var db = BuildInMemoryContext("act-2.1");

        // Seed a device with UnitId = 10
        var device = BuildAcDevice(unitId: 10);
        db.Devices.Add(device);
        await db.SaveChangesAsync();

        // No UnitOwnerProjection row for userId=99
        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id,
            Attribute: "targetTemperature",
            Value: 22,
            RequestingUserId: 99,
            RequestingUserRole: "Owner");

        var svc = BuildService(db);

        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(403, "owner not in projection → Forbidden");
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    // ── 2.2: Owner in projection but wrong unit → 403 ─────────────────────────

    /// <summary>
    /// Task 2.2 RED: Projection row exists but maps to a different UnitId than device's UnitId → Forbidden.
    /// </summary>
    [Fact]
    public async Task DeviceActuationService_OwnerInProjection_WrongUnit_Returns403()
    {
        await using var db = BuildInMemoryContext("act-2.2");

        var device = BuildAcDevice(unitId: 10);
        db.Devices.Add(device);

        // Owner's projection maps to unit 99, but device is in unit 10
        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 99,
            OwnerUserId = 77,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id,
            Attribute: "targetTemperature",
            Value: 22,
            RequestingUserId: 77,
            RequestingUserRole: "Owner");

        var svc = BuildService(db);
        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(403, "projection exists but maps to wrong unit → Forbidden");
    }

    // ── 2.3: Builder role → 403 regardless of projection ─────────────────────

    /// <summary>
    /// Task 2.3 RED: UserRole = "Builder" → Forbidden regardless of projection.
    /// </summary>
    [Fact]
    public async Task DeviceActuationService_BuilderRole_Returns403()
    {
        await using var db = BuildInMemoryContext("act-2.3");

        var device = BuildAcDevice(unitId: 5);
        db.Devices.Add(device);

        // Even if a projection row exists, Builder must be rejected
        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 5,
            OwnerUserId = 55,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id,
            Attribute: "targetTemperature",
            Value: 22,
            RequestingUserId: 55,
            RequestingUserRole: "Builder");

        var svc = BuildService(db);
        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(403, "Builder role must always get Forbidden");
    }

    // ── 2.4: Unknown attribute → 400 ──────────────────────────────────────────

    /// <summary>
    /// Task 2.4 RED: Valid owner, attribute not in DeviceCapabilityCatalog for device type → BadRequest.
    /// </summary>
    [Fact]
    public async Task DeviceActuationService_UnknownAttribute_Returns400()
    {
        await using var db = BuildInMemoryContext("act-2.4");

        var device = BuildAcDevice(unitId: 20);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 20,
            OwnerUserId = 11,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id,
            Attribute: "unknownAttr",
            Value: 50,
            RequestingUserId: 11,
            RequestingUserRole: "Owner");

        var svc = BuildService(db);
        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(400, "attribute not in catalog → BadRequest");
        result.ErrorMessage!.ToLowerInvariant().Should().Contain("attribute");
    }

    // ── 2.5: Out-of-range value → 400 ─────────────────────────────────────────

    /// <summary>
    /// Task 2.5 RED: Valid owner, targetTemperature=50 (outside 16–30) → BadRequest.
    /// </summary>
    [Fact]
    public async Task DeviceActuationService_OutOfRangeValue_Returns400()
    {
        await using var db = BuildInMemoryContext("act-2.5");

        var device = BuildAcDevice(unitId: 30);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 30,
            OwnerUserId = 22,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id,
            Attribute: "targetTemperature",
            Value: 50,   // outside [16, 30]
            RequestingUserId: 22,
            RequestingUserRole: "Owner");

        var svc = BuildService(db);
        var result = await svc.Handle(command);

        result.StatusCode.Should().Be(400, "targetTemperature=50 is outside [16,30] → BadRequest");
        result.ErrorMessage!.ToLowerInvariant().Should().Contain("range");
    }

    // ── 2.6: Valid command → shadow upserted, publish called, 200 ─────────────

    /// <summary>
    /// Task 2.6 RED: Valid owner, AC device, targetTemperature=22 → shadow upserted,
    /// IMqttPublisher.EnqueueAsync called, result is 200 with acceptedAt.
    /// </summary>
    [Fact]
    public async Task DeviceActuationService_ValidCommand_WritesShadow_EnqueuesPublish_Returns200()
    {
        await using var db = BuildInMemoryContext("act-2.6");

        var device = BuildAcDevice(unitId: 40);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 40,
            OwnerUserId = 33,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var fakePublisher = new Mock<IMqttPublisher>();
        fakePublisher
            .Setup(p => p.EnqueueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id,
            Attribute: "targetTemperature",
            Value: 22,
            RequestingUserId: 33,
            RequestingUserRole: "Owner");

        var svc = BuildService(db, fakePublisher.Object);
        var result = await svc.Handle(command);

        // Status code check
        result.StatusCode.Should().Be(200, "valid command must succeed");
        result.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Shadow must be upserted
        var shadow = await db.DeviceShadows.FindAsync(device.Id);
        shadow.Should().NotBeNull("shadow row must exist after command");
        shadow!.DesiredJson.Should().Contain("targetTemperature");
        shadow.DesiredJson.Should().Contain("22");
        shadow.UpdatedByUserId.Should().Be(33);

        // MQTT publisher must have been called with correct topic
        fakePublisher.Verify(p => p.EnqueueAsync(
            device.Id.ToString(),
            It.Is<string>(json => json.Contains("targetTemperature") && json.Contains("22")),
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Outbox row must exist for DeviceCommandIssuedEvent
        var outboxRows = db.OutboxMessages
            .Where(m => m.EventType == "DeviceCommandIssuedEvent")
            .ToList();
        outboxRows.Should().HaveCount(1, "one DeviceCommandIssuedEvent outbox row per command");
    }

    // ── W-1 fix test: shadow device_id is the natural key, not auto-incremented ──

    /// <summary>
    /// W-1 regression: shadow.DeviceId must equal the provided DeviceId (not auto-incremented).
    /// </summary>
    [Fact]
    public async Task DeviceActuationService_ShadowDeviceId_EqualsProvidedDeviceId()
    {
        await using var db = BuildInMemoryContext("act-w1");

        var device = BuildAcDevice(unitId: 50);
        db.Devices.Add(device);

        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 50,
            OwnerUserId = 44,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var command = new SendDeviceCommandCommand(
            DeviceId: device.Id,
            Attribute: "targetTemperature",
            Value: 22,
            RequestingUserId: 44,
            RequestingUserRole: "Owner");

        var svc = BuildService(db);
        await svc.Handle(command);

        var shadow = db.DeviceShadows.Single();
        shadow.DeviceId.Should().Be(device.Id,
            "shadow.DeviceId must be the natural FK — not auto-incremented (W-1 fix)");
    }
}
