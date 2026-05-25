using InfluxDB.Client;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Queries;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Infrastructure.InfluxDB;
using Microsoft.Extensions.Options;

namespace IoBuild.Devices.Application.Internal.QueryServices;

public class TelemetryQueryService : ITelemetryQueryService, IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly InfluxDbOptions _options;

    public TelemetryQueryService(IOptions<InfluxDbOptions> options)
    {
        _options = options.Value;
        var url = $"http://{_options.Host}:{_options.Port}";
        _client = new InfluxDBClient(url, _options.Token);
    }

    public async Task<IEnumerable<EnergyDataPoint>> Handle(GetDeviceEnergyQuery query)
    {
        var flux = $@"
            from(bucket: ""{_options.Bucket}"")
              |> range(start: {query.From:O}, stop: {query.To:O})
              |> filter(fn: (r) => r._measurement == ""telemetry"")
              |> filter(fn: (r) => r.deviceId == ""{query.DeviceId}"")
              |> pivot(rowKey: [""_time""], columnKey: [""_field""], valueColumn: ""_value"")
              |> keep(columns: [""_time"", ""energy_kwh"", ""temperature_c"", ""voltage_v""])";

        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(flux, _options.Org);

        var results = new List<EnergyDataPoint>();
        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                var timestamp = record.GetTime()?.ToDateTimeUtc() ?? DateTime.MinValue;
                var energy = Convert.ToDouble(record.GetValueByKey("energy_kwh") ?? 0);
                var temperature = Convert.ToDouble(record.GetValueByKey("temperature_c") ?? 0);
                var voltage = Convert.ToDouble(record.GetValueByKey("voltage_v") ?? 0);

                results.Add(new EnergyDataPoint(timestamp, energy, temperature, voltage));
            }
        }

        return results;
    }

    public async Task<DeviceStatusReport?> Handle(GetDeviceStatusQuery query)
    {
        var flux = $@"
            from(bucket: ""{_options.Bucket}"")
              |> range(start: -30d)
              |> filter(fn: (r) => r._measurement == ""telemetry"")
              |> filter(fn: (r) => r.deviceId == ""{query.DeviceId}"")
              |> last()
              |> pivot(rowKey: [""_time""], columnKey: [""_field""], valueColumn: ""_value"")
              |> keep(columns: [""_time"", ""deviceId"", ""status"", ""temperature_c"", ""voltage_v""])";

        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(flux, _options.Org);

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                var timestamp = record.GetTime()?.ToDateTimeUtc() ?? DateTime.MinValue;
                var deviceIdStr = record.GetValueByKey("deviceId")?.ToString();
                var status = record.GetValueByKey("status")?.ToString();
                var temperature = Convert.ToDouble(record.GetValueByKey("temperature_c") ?? 0);
                var voltage = Convert.ToDouble(record.GetValueByKey("voltage_v") ?? 0);

                if (status is null) continue;

                return new DeviceStatusReport(deviceIdStr is not null ? int.Parse(deviceIdStr) : query.DeviceId,
                    status, timestamp, temperature, voltage);
            }
        }

        return null;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
