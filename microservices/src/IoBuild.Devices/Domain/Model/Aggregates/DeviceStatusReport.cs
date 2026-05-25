namespace IoBuild.Devices.Domain.Model.Aggregates;

public record DeviceStatusReport(int DeviceId, string Status, DateTime LastSeen, double TemperatureC, double VoltageV);
