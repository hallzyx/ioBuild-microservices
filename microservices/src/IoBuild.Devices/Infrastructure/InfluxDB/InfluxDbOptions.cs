namespace IoBuild.Devices.Infrastructure.InfluxDB;

public class InfluxDbOptions
{
    public const string SectionName = "InfluxDb";

    public string Host { get; set; } = "influxdb";
    public int Port { get; set; } = 8086;
    public string? Token { get; set; }
    public string Org { get; set; } = "iobuild";
    public string Bucket { get; set; } = "iobuild-telemetry";
}
