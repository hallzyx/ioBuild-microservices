using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace IoBuild.Devices.Infrastructure.Mqtt;

/// <summary>
/// Singleton hosted service that holds ONE persistent MQTT connection and drains a
/// bounded <see cref="Channel{T}"/> of command messages, publishing each to
/// <c>commands/{deviceId}</c> at QoS 1 with the retain flag set (ADR-B1, ADR-B6).
///
/// Design:
/// - <see cref="IMqttPublisher.EnqueueAsync"/> writes to the channel (non-blocking for caller).
/// - The background loop drains the channel; on disconnect it reconnects before continuing.
/// - <c>ClientId = "iobuild-command-publisher"</c> (distinct from TelemetryWorker's client).
/// - TelemetryWorker is NOT touched — subscribe and publish live in separate services (SRP).
///
/// Test seam: internal constructor accepts a pre-wired <see cref="IMqttClient"/> directly
/// (mirroring FloorProvisioningConsumer / UnitDeviceProvisioningConsumer test pattern).
/// </summary>
public class MqttPublisherService : BackgroundService, IMqttPublisher
{
    private const string CommandTopicPrefix = "commands/";

    private readonly IMqttClient _client;
    private readonly MqttOptions _options;
    private readonly ILogger<MqttPublisherService> _logger;
    private readonly Channel<(string DeviceId, string PayloadJson)> _channel;

    // ── Production constructor — wired by DI ──────────────────────────────────
    public MqttPublisherService(
        IOptions<MqttOptions> mqttOptions,
        ILogger<MqttPublisherService> logger)
    {
        _options = mqttOptions.Value;
        _logger = logger;
        _client = new MqttClientFactory().CreateMqttClient();
        _channel = Channel.CreateBounded<(string, string)>(new BoundedChannelOptions(1_000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    // ── Test constructor — bypasses real broker ───────────────────────────────
    internal MqttPublisherService(
        IMqttClient client,
        ILogger<MqttPublisherService> logger)
    {
        _options = new MqttOptions
        {
            Host = "localhost",
            Port = 1883,
            CommandTopicPrefix = CommandTopicPrefix,
            PublisherClientId = "iobuild-command-publisher-test"
        };
        _logger = logger;
        _client = client;
        _channel = Channel.CreateBounded<(string, string)>(new BoundedChannelOptions(1_000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        // Start the drain loop for tests (BackgroundService.ExecuteAsync is not called in unit tests)
        _ = Task.Run(() => DrainLoopAsync(CancellationToken.None));
    }

    // ── IMqttPublisher ────────────────────────────────────────────────────────

    public async ValueTask EnqueueAsync(string deviceId, string payloadJson, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync((deviceId, payloadJson), ct);
    }

    // ── BackgroundService ─────────────────────────────────────────────────────

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MqttPublisherService starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnsureConnectedAsync(stoppingToken);
                await DrainLoopAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MqttPublisherService: connection error — reconnecting in 10 s...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("MqttPublisherService stopped.");
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_client.IsConnected)
            return;

        var connectOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(_options.PublisherClientId)
            .WithCleanSession()
            .Build();

        if (!string.IsNullOrEmpty(_options.Username))
        {
            connectOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Host, _options.Port)
                .WithClientId(_options.PublisherClientId)
                .WithCredentials(_options.Username, _options.Password)
                .WithCleanSession()
                .Build();
        }

        _logger.LogInformation(
            "MqttPublisherService: connecting to {Host}:{Port} as {ClientId}...",
            _options.Host, _options.Port, _options.PublisherClientId);

        await _client.ConnectAsync(connectOptions, ct);
        _logger.LogInformation("MqttPublisherService: connected.");
    }

    private async Task DrainLoopAsync(CancellationToken ct)
    {
        await foreach (var (deviceId, payloadJson) in _channel.Reader.ReadAllAsync(ct))
        {
            try
            {
                await EnsureConnectedAsync(ct);

                var topic = $"{_options.CommandTopicPrefix}{deviceId}";
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(Encoding.UTF8.GetBytes(payloadJson))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(true)
                    .Build();

                await _client.PublishAsync(message, ct);

                _logger.LogDebug(
                    "MqttPublisherService: published to {Topic} payload={Payload}",
                    topic, payloadJson);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "MqttPublisherService: failed to publish command for device {DeviceId}. Message re-queued.",
                    deviceId);
                // Re-enqueue so the message is not lost (best-effort; channel may be full under pressure)
                try { await _channel.Writer.WriteAsync((deviceId, payloadJson), ct); }
                catch { /* channel full or cancelled — accept loss */ }
            }
        }
    }
}
