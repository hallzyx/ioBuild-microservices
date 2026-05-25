namespace IoBuild.Devices.Domain.Model.Aggregates;

public record EnergyDataPoint(DateTime Timestamp, double EnergyKwh, double TemperatureC, double VoltageV);
