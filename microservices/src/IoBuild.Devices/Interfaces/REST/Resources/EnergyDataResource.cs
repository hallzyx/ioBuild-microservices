namespace IoBuild.Devices.Interfaces.REST.Resources;

public record EnergyDataResource(DateTime Timestamp, double EnergyKwh, double TemperatureC, double VoltageV);
