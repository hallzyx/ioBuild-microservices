namespace IoBuild.Devices.Domain.Model.Commands;

public record CreateDeviceCommand(string Name, string Type, string Location, string MacAddress, int ProjectId, string Status);
