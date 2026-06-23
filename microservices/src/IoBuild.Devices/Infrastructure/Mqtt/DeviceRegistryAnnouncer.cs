using System.Text.Json;
using IoBuild.Devices.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoBuild.Devices.Infrastructure.Mqtt;

/// <summary>
/// Startup background service that publishes a retained <c>registry/{deviceId}</c> message for every
/// device currently in the DB, so the IoT simulator can discover all existing devices on (re)connect.
///
/// This is load-bearing — NOT covered by the OutboxWorker hook alone: HasData seeds emit
/// DeviceCreatedEvent only when the outbox is empty (OutboxBackfill), and the broker may lose
/// retained messages (e.g. `docker compose down -v`). Re-announcing all devices on every startup
/// reconciles both cases. Enqueues through the channel, so it does not require the MQTT connection
/// to be up yet.
/// </summary>
public class DeviceRegistryAnnouncer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMqttPublisher _mqtt;
    private readonly ILogger<DeviceRegistryAnnouncer> _logger;

    public DeviceRegistryAnnouncer(
        IServiceScopeFactory scopeFactory,
        IMqttPublisher mqtt,
        ILogger<DeviceRegistryAnnouncer> logger)
    {
        _scopeFactory = scopeFactory;
        _mqtt = mqtt;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await AnnounceAllAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeviceRegistryAnnouncer: failed to announce devices on startup.");
        }
    }

    internal async Task AnnounceAllAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
        var devices = (await repo.ListAsync()).ToList();

        // Enqueues to the publisher's bounded channel (capacity 1000); the drain loop connects lazily, so this does not block on broker readiness. Assumes startup device count < channel capacity.
        foreach (var d in devices)
        {
            var json = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["deviceId"] = d.Id,
                ["type"] = d.Type
            });
            await _mqtt.EnqueueRawAsync($"registry/{d.Id}", json, true, ct);
        }

        _logger.LogInformation("DeviceRegistryAnnouncer: announced {Count} devices to registry/#.", devices.Count);
    }
}
