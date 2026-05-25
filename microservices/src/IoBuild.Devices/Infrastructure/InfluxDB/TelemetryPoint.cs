using InfluxDB.Client.Core;

namespace IoBuild.Devices.Infrastructure.InfluxDB;

[Measurement("telemetry")]
public class TelemetryPoint
{
    [Column("deviceId", IsTag = true)]
    public string DeviceId { get; set; } = string.Empty;

    [Column("location", IsTag = true)]
    public string Location { get; set; } = string.Empty;

    [Column("energy_kwh")]
    public double EnergyKwh { get; set; }

    [Column("temperature_c")]
    public double TemperatureC { get; set; }

    [Column("voltage_v")]
    public double VoltageV { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }
}
