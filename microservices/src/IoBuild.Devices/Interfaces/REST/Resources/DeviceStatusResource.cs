namespace IoBuild.Devices.Interfaces.REST.Resources;

public record DeviceStatusResource(
    int DeviceId,
    string Status,
    DateTime LastSeen,
    double TemperatureC,
    double VoltageV,
    Dictionary<string, object>? Desired = null);
