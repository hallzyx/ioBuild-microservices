using InfluxDB.Client;
using Microsoft.Extensions.Options;

namespace IoBuild.Analytics.Infrastructure.InfluxDB;

public interface ILiveDeviceStatusService
{
    /// <summary>
    /// Returns the latest telemetry-reported status ("online"/"idle"/...) per device,
    /// keyed by deviceId. Devices with no telemetry point yet are omitted from the
    /// result — callers should fall back to their own projection's static status.
    /// </summary>
    Task<Dictionary<string, string>> GetLatestStatusesAsync(IEnumerable<string> deviceIds);
}

public class LiveDeviceStatusService : ILiveDeviceStatusService, IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly InfluxDbOptions _options;

    public LiveDeviceStatusService(IOptions<InfluxDbOptions> options)
    {
        _options = options.Value;
        var url = $"http://{_options.Host}:{_options.Port}";
        _client = new InfluxDBClient(url, _options.Token);
    }

    public async Task<Dictionary<string, string>> GetLatestStatusesAsync(IEnumerable<string> deviceIds)
    {
        var ids = deviceIds.ToList();
        if (ids.Count == 0)
            return new Dictionary<string, string>();

        var idFilter = string.Join(" or ", ids.Select(id => $@"r.deviceId == ""{id}"""));

        var flux = $@"
from(bucket: ""{_options.Bucket}"")
  |> range(start: -30d)
  |> filter(fn: (r) => r._measurement == ""telemetry"" and r._field == ""status"")
  |> filter(fn: (r) => {idFilter})
  |> group(columns: [""deviceId""])
  |> last()";

        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(flux, _options.Org);

        var results = new Dictionary<string, string>();
        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                var deviceId = record.GetValueByKey("deviceId")?.ToString();
                var status = record.GetValue()?.ToString();
                if (deviceId is not null && status is not null)
                    results[deviceId] = status;
            }
        }

        return results;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
