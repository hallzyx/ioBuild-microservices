namespace IoBuild.Devices.Domain.Model.Queries;

public record GetDeviceEnergyQuery(int DeviceId, DateTime From, DateTime To);
