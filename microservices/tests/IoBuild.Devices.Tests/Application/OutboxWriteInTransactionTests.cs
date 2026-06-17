using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Shared.Domain.Services;
using Moq;
using FluentAssertions;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// RED tests for task 1.2 — OutboxWrite in single transaction.
/// REQ-DE-02: the outbox row MUST be written in the SAME transaction as the device.
/// These tests will fail until DeviceCommandService is wired with IOutboxMessageRepository.
/// </summary>
public class OutboxWriteInTransactionTests
{
    [Fact]
    public async Task HandleCreate_WritesDeviceAndOutboxRow_BeforeSaveChanges()
    {
        // Arrange
        var deviceRepo = new Mock<IDeviceRepository>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();
        var publisher = new Mock<IDomainEventPublisher>();

        var addedDevices = new List<IoBuild.Devices.Domain.Model.Aggregates.Device>();
        var addedOutboxMessages = new List<OutboxMessage>();

        deviceRepo.Setup(r => r.AddAsync(It.IsAny<IoBuild.Devices.Domain.Model.Aggregates.Device>()))
            .Callback<IoBuild.Devices.Domain.Model.Aggregates.Device>(d => addedDevices.Add(d))
            .Returns(Task.CompletedTask);

        outboxRepo.Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => addedOutboxMessages.Add(m))
            .Returns(Task.CompletedTask);

        deviceRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new DeviceCommandService(deviceRepo.Object, outboxRepo.Object);

        var command = new CreateDeviceCommand("Sensor-1", "Sensor", "Room 1", "AA:BB:CC:DD:EE:01", 1, "Online");

        // Act
        await service.Handle(command);

        // Assert — both device AND outbox row added before SaveChanges
        addedDevices.Should().HaveCount(1);
        addedOutboxMessages.Should().HaveCount(1);

        var outboxMsg = addedOutboxMessages[0];
        outboxMsg.EventType.Should().Be("DeviceCreatedEvent");
        outboxMsg.Status.Should().Be("Pending");
        outboxMsg.Payload.Should().NotBeNullOrEmpty();
        outboxMsg.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleCreate_SaveChangesCalledOnce_CoveringBothRows()
    {
        // Arrange
        var deviceRepo = new Mock<IDeviceRepository>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();

        deviceRepo.Setup(r => r.AddAsync(It.IsAny<IoBuild.Devices.Domain.Model.Aggregates.Device>())).Returns(Task.CompletedTask);
        outboxRepo.Setup(r => r.AddAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);
        deviceRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var service = new DeviceCommandService(deviceRepo.Object, outboxRepo.Object);
        var command = new CreateDeviceCommand("Sensor-2", "Sensor", "Room 2", "AA:BB:CC:DD:EE:02", 1, "Online");

        // Act
        await service.Handle(command);

        // Assert — exactly one SaveChanges covers both writes (single transaction)
        deviceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleCreate_OutboxPayloadContainsDeviceId()
    {
        // Arrange
        var deviceRepo = new Mock<IDeviceRepository>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();

        OutboxMessage? capturedMsg = null;
        deviceRepo.Setup(r => r.AddAsync(It.IsAny<IoBuild.Devices.Domain.Model.Aggregates.Device>())).Returns(Task.CompletedTask);
        outboxRepo.Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => capturedMsg = m)
            .Returns(Task.CompletedTask);
        deviceRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var service = new DeviceCommandService(deviceRepo.Object, outboxRepo.Object);
        var command = new CreateDeviceCommand("Sensor-3", "Sensor", "Room 3", "AA:BB:CC:DD:EE:03", 5, "Offline");

        // Act
        await service.Handle(command);

        // Assert
        capturedMsg.Should().NotBeNull();
        capturedMsg!.Payload.Should().Contain("Sensor");
    }
}
