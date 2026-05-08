namespace IoBuild.Devices.Interfaces.REST.Resources;

public record CreateDeviceResource(string Name, string Type, string Location, string MacAddress, int ProjectId, string Status);
