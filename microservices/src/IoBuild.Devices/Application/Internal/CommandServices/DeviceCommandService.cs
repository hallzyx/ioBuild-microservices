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

        // ADR-A two-phase commit — same pattern as UnitCommandService (§7.3):
        //   Phase 1: persist the device → EF/MySQL assigns the real identity (device.Id is 0
        //            until this call returns on a MySQL identity column).
        await repository.SaveChangesAsync();

        // Phase 2: build the domain event with the real device.Id, persist outbox row.
        //   If the process crashes between the two SaveChanges calls, the device exists
        //   without an outbox row. OutboxBackfill.RunAsync (run at startup) re-emits
        //   DeviceCreatedEvent for any seeded/orphaned devices — the same safety net as Projects.
        var evt = new DeviceCreatedEvent
        {
            DeviceId = device.Id,   // real Id after phase-1 commit
            OwnerUserId = 0,
            ProjectId = device.ProjectId,
            DeviceType = device.Type,
            Status = device.Status,
            FloorNumber = device.FloorNumber
        };

        var payload = JsonSerializer.Serialize(evt);
        var outboxMessage = new OutboxMessage(nameof(DeviceCreatedEvent), payload)
        {
            EventId = evt.EventId
        };

        await outboxRepository.AddAsync(outboxMessage);
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
