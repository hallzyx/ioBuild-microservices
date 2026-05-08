namespace IoBuild.Analytics.Interfaces.ACL;

public interface IDevicesContextFacade
{
    Task<IEnumerable<Dictionary<string, object>>> GetDevicesByProjectAsync(int projectId);
    Task<IEnumerable<Dictionary<string, object>>> GetDevicesByUserAsync(int userId);
    Task<Dictionary<string, object>?> GetDeviceTelemetryAsync(int deviceId, string metric, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Dictionary<string, object>>> GetDeviceHealthStatusesAsync(int userId);
    Task<int> GetTotalDevicesAsync(int userId);
    Task<int> GetOnlineDevicesAsync(int userId);
    Task<int> GetOfflineDevicesAsync(int userId);
    Task<int> GetAlertsCountAsync(int userId);
    Task<Dictionary<string, int>> GetDevicesByTypeAsync(int userId);
    Task<double> GetTemperatureAverageAsync(int userId);
    Task<double> GetEnergyThisMonthAsync(int userId);
    Task<double> GetWaterUsageThisMonthAsync(int userId);
}
