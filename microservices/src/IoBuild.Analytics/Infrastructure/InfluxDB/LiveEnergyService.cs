using InfluxDB.Client;
using Microsoft.Extensions.Options;

namespace IoBuild.Analytics.Infrastructure.InfluxDB;

public record EnergyMinutePoint(DateTime Timestamp, double TotalEnergyKwh);

public interface ILiveEnergyService
{
    Task<IEnumerable<EnergyMinutePoint>> GetAggregatedAsync(IEnumerable<string> deviceIds, int minutes);
}

public class LiveEnergyService : ILiveEnergyService, IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly InfluxDbOptions _options;

    public LiveEnergyService(IOptions<InfluxDbOptions> options)
    {
        _options = options.Value;
        var url = $"http://{_options.Host}:{_options.Port}";
        _client = new InfluxDBClient(url, _options.Token);
    }

    public async Task<IEnumerable<EnergyMinutePoint>> GetAggregatedAsync(
        IEnumerable<string> deviceIds, int minutes)
    {
        var ids = deviceIds.ToList();
        if (ids.Count == 0)
            return [];

        // Build OR filter for deviceIds
        var idFilter = string.Join(" or ", ids.Select(id => $@"r.deviceId == ""{id}"""));

        var flux = $@"
from(bucket: ""{_options.Bucket}"")
  |> range(start: -{minutes}m)
  |> filter(fn: (r) => r._measurement == ""telemetry"" and r._field == ""energy_kwh"")
  |> filter(fn: (r) => {idFilter})
  |> aggregateWindow(every: 1m, fn: mean, createEmpty: false)
  |> group(columns: [""_time""])
  |> sum()";

        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(flux, _options.Org);

        var results = new List<EnergyMinutePoint>();
        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                var timestamp = record.GetTime()?.ToDateTimeUtc() ?? DateTime.MinValue;
                var value = Math.Round(Convert.ToDouble(record.GetValue() ?? 0), 2);
                results.Add(new EnergyMinutePoint(timestamp, value));
            }
        }

        return results.OrderBy(p => p.Timestamp);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
