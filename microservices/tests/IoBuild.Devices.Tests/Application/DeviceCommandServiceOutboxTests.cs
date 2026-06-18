using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// RED→GREEN test for task 6.1 / 6.2 — DeviceCommandService Id==0 fix (§7.3 / ADR-A).
///
/// The bug: DeviceCreatedEvent is built with DeviceId = device.Id BEFORE SaveChangesAsync,
/// so the outbox payload carries DeviceId=0 on MySQL (identity column is 0 until DB INSERT).
///
/// The fix (two-phase commit): SaveChangesAsync first (real Id assigned), THEN build
/// DeviceCreatedEvent with device.Id, THEN add outbox row, THEN second SaveChangesAsync.
///
/// EF InMemory assigns positive sequential Ids after AddAsync (not after SaveChanges),
/// so we can't distinguish the phases via Id alone on InMemory. We instead verify ordering
/// by checking that the captured DeviceId in the outbox payload matches the actual device.Id
/// as observed from the database — which confirms the event was built with the post-save Id.
///
/// Note (same as PR1 discovery): EF InMemory does not replicate the MySQL Id=0 pre-save
/// scenario. The canonical RED for the two-phase ordering is the Moq-based seam test
/// (HandleCreate_SaveChangesCalledTwice_TwoPhaseCommit). The EF-InMemory test proves the
/// payload Id is consistent with what EF reports.
/// </summary>
public class DeviceCommandServiceOutboxTests
{
    private static DevicesDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new DevicesDbContext(options);
    }

    /// <summary>
    /// FD — DeviceCommandService: the outbox payload DeviceId must equal the device's persisted Id.
    /// RED: fails if event is built before SaveChanges (Id=0 path) or payload is inconsistent.
    /// </summary>
    [Fact]
    public async Task HandleCreate_OutboxPayload_DeviceId_MatchesPersisted_Id()
    {
        // Arrange
        var dbName = nameof(HandleCreate_OutboxPayload_DeviceId_MatchesPersisted_Id);
        await using var ctx = BuildContext(dbName);

        var deviceRepo = new DeviceRepository(ctx);
        var outboxRepo = new OutboxMessageRepository(ctx);
        var service = new DeviceCommandService(deviceRepo, outboxRepo);

        var command = new CreateDeviceCommand(
            "Floor SmartMeter", "SmartMeter", "Floor 1", "AA:BB:CC:DD:EE:F1", 10, "Active");

        // Act
        var device = await service.Handle(command);

        // Assert: device got a real Id (InMemory assigns it)
        device.Id.Should().BeGreaterThan(0, "EF should assign a positive Id on Add/Save");

        // Assert: outbox row exists with matching DeviceId in payload
        var outboxRows = await ctx.OutboxMessages.ToListAsync();
        outboxRows.Should().HaveCount(1);
        outboxRows[0].EventType.Should().Be("DeviceCreatedEvent");

        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(outboxRows[0].Payload)!;
        payload["DeviceId"].GetInt32().Should().Be(device.Id,
            "DeviceCreatedEvent.DeviceId in outbox must equal the device's real persisted Id (two-phase commit fix)");
    }

    /// <summary>
    /// Moq-based seam: verifies SaveChangesAsync is called TWICE (two-phase commit).
    /// This is the canonical RED for the ordering fix — fails on old single-commit code.
    /// </summary>
    [Fact]
    public async Task HandleCreate_SaveChangesCalledTwice_TwoPhaseCommit()
    {
        // Arrange
        var deviceRepo = new Moq.Mock<IoBuild.Devices.Domain.Repositories.IDeviceRepository>();
        var outboxRepo = new Moq.Mock<IoBuild.Devices.Domain.Repositories.IOutboxMessageRepository>();

        int saveChangesCallCount = 0;

        deviceRepo.Setup(r => r.AddAsync(Moq.It.IsAny<IoBuild.Devices.Domain.Model.Aggregates.Device>()))
            .Returns(Task.CompletedTask);
        outboxRepo.Setup(r => r.AddAsync(Moq.It.IsAny<OutboxMessage>()))
            .Returns(Task.CompletedTask);
        deviceRepo.Setup(r => r.SaveChangesAsync())
            .Callback(() => saveChangesCallCount++)
            .Returns(Task.CompletedTask);

        var service = new DeviceCommandService(deviceRepo.Object, outboxRepo.Object);
        var command = new CreateDeviceCommand(
            "Floor SmartMeter 2", "SmartMeter", "Floor 2", "AA:BB:CC:DD:EE:F2", 10, "Active");

        // Act
        await service.Handle(command);

        // Assert: SaveChangesAsync must be called exactly twice (two-phase commit)
        saveChangesCallCount.Should().Be(2,
            "Two-phase commit: first SaveChanges persists the device (real Id), " +
            "second SaveChanges persists the outbox row (ADR-A, §7.3)");
    }

    /// <summary>
    /// Verify that the outbox row is added AFTER the first SaveChanges in the two-phase commit.
    /// Confirmed by checking that AddAsync(outboxMessage) is called AFTER SaveChangesAsync.
    /// </summary>
    [Fact]
    public async Task HandleCreate_OutboxRowAddedAfterFirstSave_CorrectPhaseOrder()
    {
        // Arrange
        var deviceRepo = new Moq.Mock<IoBuild.Devices.Domain.Repositories.IDeviceRepository>();
        var outboxRepo = new Moq.Mock<IoBuild.Devices.Domain.Repositories.IOutboxMessageRepository>();

        var callOrder = new List<string>();

        deviceRepo.Setup(r => r.AddAsync(Moq.It.IsAny<IoBuild.Devices.Domain.Model.Aggregates.Device>()))
            .Callback(() => callOrder.Add("AddDevice"))
            .Returns(Task.CompletedTask);

        deviceRepo.Setup(r => r.SaveChangesAsync())
            .Callback(() => callOrder.Add("SaveChanges"))
            .Returns(Task.CompletedTask);

        outboxRepo.Setup(r => r.AddAsync(Moq.It.IsAny<OutboxMessage>()))
            .Callback(() => callOrder.Add("AddOutbox"))
            .Returns(Task.CompletedTask);

        var service = new DeviceCommandService(deviceRepo.Object, outboxRepo.Object);
        var command = new CreateDeviceCommand(
            "Floor WaterSensor", "WaterSensor", "Floor 3", "AA:BB:CC:DD:EE:F3", 10, "Active");

        // Act
        await service.Handle(command);

        // Assert: AddDevice → SaveChanges → AddOutbox → SaveChanges
        callOrder.Should().ContainInOrder(
            "AddDevice", "SaveChanges", "AddOutbox", "SaveChanges");

        callOrder.Should().HaveCount(4,
            "Expected exactly: AddDevice, SaveChanges(phase1), AddOutbox, SaveChanges(phase2)");
    }
}
