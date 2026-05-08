using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Domain.Services;

namespace IoBuild.Devices.Application.Internal.CommandServices;

public class DeviceCommandService(IDeviceRepository repository) : IDeviceCommandService
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
        await repository.SaveChangesAsync();

        return device;
    }

    public async Task Handle(DeleteDeviceCommand command)
    {
        var device = await repository.FindByIdAsync(command.Id);

        if (device is null)
            throw new KeyNotFoundException($"Device with id {command.Id} not found.");

        repository.Remove(device);
        await repository.SaveChangesAsync();
    }
}
