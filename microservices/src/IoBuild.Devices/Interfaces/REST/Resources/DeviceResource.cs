namespace IoBuild.Devices.Interfaces.REST.Resources;

public record DeviceResource(int Id, string Name, string Type, string Location, string MacAddress, int ProjectId, string Status);
