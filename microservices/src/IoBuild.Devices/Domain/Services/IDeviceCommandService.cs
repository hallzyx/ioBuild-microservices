using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;

namespace IoBuild.Devices.Domain.Services;

public interface IDeviceCommandService
{
    Task<Device> Handle(CreateDeviceCommand command);
    Task<Device> Handle(UpdateDeviceCommand command);
    Task Handle(DeleteDeviceCommand command);
}
