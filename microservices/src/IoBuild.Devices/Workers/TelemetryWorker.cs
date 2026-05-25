using System.Buffers;
using System.Text;
using System.Text.Json;
using IoBuild.Devices.Infrastructure.InfluxDB;
using IoBuild.Devices.Infrastructure.Mqtt;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace IoBuild.Devices.Workers;

public class TelemetryWorker : BackgroundService
{
    private readonly ITelemetryWriteService _writeService;
    private readonly MqttOptions _mqttOptions;
    private readonly ILogger<TelemetryWorker> _logger;
    private readonly MqttClientFactory _factory;

    public TelemetryWorker(
        ITelemetryWriteService writeService,
        IOptions<MqttOptions> mqttOptions,
        ILogger<TelemetryWorker> logger)
    {
        _writeService = writeService;
        _mqttOptions = mqttOptions.Value;
        _logger = logger;
        _factory = new MqttClientFactory();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TelemetryWorker starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndSubscribeAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "MQTT connection error. Reconnecting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken ct)
    {
        using var mqttClient = _factory.CreateMqttClient();

        var connectOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttOptions.Host, _mqttOptions.Port)
            .WithClientId(_mqttOptions.ClientId)
            .WithCleanSession()
            .Build();

        mqttClient.ApplicationMessageReceivedAsync += async args =>
        {
            await HandleMessageAsync(args, ct);
        };

        _logger.LogInformation("Connecting to MQTT broker at {Host}:{Port}...",
            _mqttOptions.Host, _mqttOptions.Port);

        await mqttClient.ConnectAsync(connectOptions, ct);

        _logger.LogInformation("Connected to MQTT. Subscribing to {Topic}...",
            _mqttOptions.Topic);

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(
                _mqttOptions.Topic,
                MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await mqttClient.SubscribeAsync(subscribeOptions, ct);

        _logger.LogInformation("Subscribed to {Topic}. Waiting for messages...",
            _mqttOptions.Topic);

        await Task.Delay(Timeout.Infinite, ct);
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs args, CancellationToken ct)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload.ToArray());
            _logger.LogDebug("Received MQTT message on {Topic}", args.ApplicationMessage.Topic);

            var raw = JsonSerializer.Deserialize<TelemetryRawPayload>(payload);
            if (raw is null)
            {
                _logger.LogWarning("Failed to deserialize payload: {Payload}", payload);
                return;
            }

            var point = new TelemetryPoint
            {
                DeviceId = raw.deviceId.ToString(),
                Location = raw.location ?? "unknown",
                EnergyKwh = raw.energy_kwh,
                TemperatureC = raw.temperature_c,
                VoltageV = raw.voltage_v,
                Status = raw.status ?? "unknown",
                Timestamp = raw.timestamp
            };

            _logger.LogDebug("Energy value for device {DeviceId}: {Energy}", raw.deviceId, raw.energy_kwh);

            await _writeService.WriteAsync(point, ct);
            _logger.LogDebug("Written telemetry for device {DeviceId}", raw.deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message");
        }
    }

    private record TelemetryRawPayload(
        int deviceId,
        DateTime timestamp,
        double energy_kwh,
        double temperature_c,
        double voltage_v,
        string? status,
        string? location);
}
