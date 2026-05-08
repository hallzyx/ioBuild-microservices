namespace IoBuild.Devices.Domain.Model.Commands;

public record UpdateDeviceCommand(int Id, string Name, string Type, string Location, string MacAddress, int ProjectId, string Status);
