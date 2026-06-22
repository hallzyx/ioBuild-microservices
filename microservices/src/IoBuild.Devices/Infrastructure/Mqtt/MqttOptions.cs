namespace IoBuild.Devices.Infrastructure.Mqtt;

public class MqttOptions
{
    public const string SectionName = "Mqtt";

    public string Host { get; set; } = "mosquitto";
    public int Port { get; set; } = 1883;
    public string Topic { get; set; } = "telemetry/#";
    public string ClientId { get; set; } = "iobuild-telemetry-worker";

    // ── Publisher additions (ADR-B1, ADR-B6) ──────────────────────────────────
    /// <summary>Topic prefix for outbound device commands (e.g. "commands/").</summary>
    public string CommandTopicPrefix { get; set; } = "commands/";

    /// <summary>ClientId for the publisher service — must differ from TelemetryWorker.</summary>
    public string PublisherClientId { get; set; } = "iobuild-command-publisher";

    /// <summary>Optional MQTT username (null = anonymous, matches mosquitto default).</summary>
    public string? Username { get; set; }

    /// <summary>Optional MQTT password; only used when <see cref="Username"/> is set.</summary>
    public string? Password { get; set; }
}
