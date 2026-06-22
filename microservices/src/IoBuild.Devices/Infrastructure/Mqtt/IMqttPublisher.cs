namespace IoBuild.Devices.Infrastructure.Mqtt;

/// <summary>
/// Channel-backed facade for publishing MQTT command messages to devices.
/// Callers enqueue a payload; the singleton <see cref="MqttPublisherService"/> drains
/// the channel asynchronously over a persistent connection (ADR-B1).
/// </summary>
public interface IMqttPublisher
{
    /// <summary>
    /// Enqueues a JSON payload for immediate publish to <c>commands/{deviceId}</c>.
    /// Returns as soon as the message is accepted into the internal channel;
    /// the actual MQTT publish happens asynchronously on the background service's drain loop.
    /// </summary>
    ValueTask EnqueueAsync(string deviceId, string payloadJson, CancellationToken ct = default);
}
