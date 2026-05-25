using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;

namespace IoBuild.Devices.Infrastructure.InfluxDB;

public class TelemetryWriteService : ITelemetryWriteService, IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly InfluxDbOptions _options;

    public TelemetryWriteService(IOptions<InfluxDbOptions> options)
    {
        _options = options.Value;
        var url = $"http://{_options.Host}:{_options.Port}";
        _client = new InfluxDBClient(url, _options.Token);
    }

    public async Task WriteAsync(TelemetryPoint point, CancellationToken ct = default)
    {
        using var writeApi = _client.GetWriteApi();
        var line = PointData.Measurement("telemetry")
            .Tag("deviceId", point.DeviceId)
            .Tag("location", point.Location)
            .Field("energy_kwh", point.EnergyKwh)
            .Field("temperature_c", point.TemperatureC)
            .Field("voltage_v", point.VoltageV)
            .Field("status", point.Status)
            .Timestamp(point.Timestamp.ToUniversalTime(), WritePrecision.Ns);
        writeApi.WritePoint(line, _options.Bucket, _options.Org);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
