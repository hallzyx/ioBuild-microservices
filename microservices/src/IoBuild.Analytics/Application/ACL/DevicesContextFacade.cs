using IoBuild.Analytics.Interfaces.ACL;

namespace IoBuild.Analytics.Application.ACL;

public class DevicesContextFacade : IDevicesContextFacade
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DevicesContextFacade> _logger;

    public DevicesContextFacade(HttpClient httpClient, ILogger<DevicesContextFacade> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetDevicesByProjectAsync(int projectId)
    {
        _logger.LogInformation("Fetching devices for project {ProjectId}", projectId);
        return await CallDevicesApiAsync($"/api/devices?projectId={projectId}");
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetDevicesByUserAsync(int userId)
    {
        _logger.LogInformation("Fetching devices for user {UserId}", userId);
        return await CallDevicesApiAsync($"/api/devices?userId={userId}");
    }

    public async Task<Dictionary<string, object>?> GetDeviceTelemetryAsync(int deviceId, string metric, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Fetching telemetry for device {DeviceId}, metric {Metric}", deviceId, metric);
        var results = await CallDevicesApiAsync($"/api/devices/{deviceId}/telemetry?metric={metric}&startDate={startDate:O}&endDate={endDate:O}");
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetDeviceHealthStatusesAsync(int userId)
    {
        _logger.LogInformation("Fetching device health for user {UserId}", userId);
        return await CallDevicesApiAsync($"/api/devices/health?userId={userId}");
    }

    public async Task<int> GetTotalDevicesAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/total-devices");
        return ExtractCount(data);
    }

    public async Task<int> GetOnlineDevicesAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/online-devices");
        return ExtractCount(data);
    }

    public async Task<int> GetOfflineDevicesAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/offline-devices");
        return ExtractCount(data);
    }

    public async Task<int> GetAlertsCountAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/alerts-count");
        return ExtractCount(data);
    }

    public async Task<Dictionary<string, int>> GetDevicesByTypeAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/devices-by-type");
        return data.ToDictionary(d => d["type"]?.ToString() ?? "unknown", d => Convert.ToInt32(d["count"]));
    }

    public async Task<double> GetTemperatureAverageAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/temperature-avg");
        return ExtractDouble(data);
    }

    public async Task<double> GetEnergyThisMonthAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/energy-month");
        return ExtractDouble(data);
    }

    public async Task<double> GetWaterUsageThisMonthAsync(int userId)
    {
        var data = await CallDevicesApiAsync($"/api/analytics/user/{userId}/water-month");
        return ExtractDouble(data);
    }

    private async Task<IEnumerable<Dictionary<string, object>>> CallDevicesApiAsync(string path)
    {
        try
        {
            var response = await _httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<Dictionary<string, object>>>();
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Devices API at {Path}", path);
            return [];
        }
    }

    private static int ExtractCount(IEnumerable<Dictionary<string, object>> data)
    {
        var first = data.FirstOrDefault();
        return first != null && first.TryGetValue("count", out var count) ? Convert.ToInt32(count) : 0;
    }

    private static double ExtractDouble(IEnumerable<Dictionary<string, object>> data)
    {
        var first = data.FirstOrDefault();
        return first != null && first.TryGetValue("value", out var val) ? Convert.ToDouble(val) : 0.0;
    }
}
