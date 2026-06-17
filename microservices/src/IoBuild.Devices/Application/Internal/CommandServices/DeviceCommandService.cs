using System.Text.Json;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Domain.Services;
using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Devices.Application.Internal.CommandServices;

public class DeviceCommandService(
    IDeviceRepository repository,
    IOutboxMessageRepository outboxRepository) : IDeviceCommandService
{
    public async Task<Device> Handle(CreateDeviceCommand command)
    {
        var device = new Device(
            command.Name,
            command.Type,
            command.Location,
            command.MacAddress,
            command.ProjectId,
            command.Status);

        await repository.AddAsync(device);

        // Build and serialize the domain event for the outbox (ADR-8, REQ-DE-02)
        var evt = new DeviceCreatedEvent
        {
            // Note: Device.Id is 0 until SaveChanges assigns it (identity column).
            // The OutboxWorker will have the correct Id in the payload after commit.
            // OwnerUserId is not on the Device aggregate yet; mapped as 0.
            // TODO (batch 2): map OwnerUserId once Device carries it.
            DeviceId = device.Id,
            OwnerUserId = 0,
            ProjectId = device.ProjectId,
            DeviceType = device.Type,
            Status = device.Status
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(DeviceCreatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await outboxRepository.AddAsync(outboxMessage);

        // Single SaveChanges covers BOTH the device row and the outbox row (REQ-DE-02)
        await repository.SaveChangesAsync();

        return device;
    }

    public async Task<Device> Handle(UpdateDeviceCommand command)
    {
        var device = await repository.FindByIdAsync(command.Id);

        if (device is null)
            throw new KeyNotFoundException($"Device with id {command.Id} not found.");

        device.Update(
            command.Name,
            command.Type,
            command.Location,
            command.MacAddress,
            command.ProjectId,
            command.Status);

        repository.Update(device);

        var evt = new DeviceUpdatedEvent
        {
            DeviceId = device.Id,
            OwnerUserId = 0,
            ProjectId = device.ProjectId,
            DeviceType = device.Type,
            Status = device.Status
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(DeviceUpdatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await outboxRepository.AddAsync(outboxMessage);
        await repository.SaveChangesAsync();

        return device;
    }

    public async Task Handle(DeleteDeviceCommand command)
    {
        var device = await repository.FindByIdAsync(command.Id);

        if (device is null)
            throw new KeyNotFoundException($"Device with id {command.Id} not found.");

        var evt = new DeviceDeletedEvent
        {
            DeviceId = device.Id,
            OwnerUserId = 0
        };

        repository.Remove(device);

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(DeviceDeletedEvent), payload)
        {
            EventId = evt.EventId
        };

        await outboxRepository.AddAsync(outboxMessage);
        await repository.SaveChangesAsync();
    }
}
