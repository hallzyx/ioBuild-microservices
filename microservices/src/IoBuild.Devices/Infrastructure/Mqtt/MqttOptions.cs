namespace IoBuild.Devices.Infrastructure.Mqtt;

public class MqttOptions
{
    public const string SectionName = "Mqtt";

    public string Host { get; set; } = "mosquitto";
    public int Port { get; set; } = 1883;
    public string Topic { get; set; } = "telemetry/#";
    public string ClientId { get; set; } = "iobuild-telemetry-worker";
}
