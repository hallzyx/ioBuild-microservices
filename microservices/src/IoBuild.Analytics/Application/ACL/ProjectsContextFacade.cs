using IoBuild.Analytics.Interfaces.ACL;

namespace IoBuild.Analytics.Application.ACL;

public class ProjectsContextFacade : IProjectsContextFacade
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectsContextFacade> _logger;

    public ProjectsContextFacade(HttpClient httpClient, ILogger<ProjectsContextFacade> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetProjectsByBuilderAsync(int userId)
    {
        _logger.LogInformation("Fetching projects for builder {UserId}", userId);
        return await CallProjectsApiAsync($"/api/projects?builderId={userId}");
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetUnitsByOwnerAsync(int userId)
    {
        _logger.LogInformation("Fetching units for owner {UserId}", userId);
        return await CallProjectsApiAsync($"/api/units?ownerId={userId}");
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetUnitsByProjectAsync(int projectId)
    {
        _logger.LogInformation("Fetching units for project {ProjectId}", projectId);
        return await CallProjectsApiAsync($"/api/projects/{projectId}/units");
    }

    public async Task<int> GetActiveProjectsCountAsync(int userId)
    {
        var data = await CallProjectsApiAsync($"/api/analytics/builder/{userId}/active-projects");
        return ExtractCount(data);
    }

    public async Task<int> GetTotalUnitsAsync(int userId)
    {
        var data = await CallProjectsApiAsync($"/api/analytics/builder/{userId}/total-units");
        return ExtractCount(data);
    }

    public async Task<int> GetOccupiedUnitsAsync(int userId)
    {
        var data = await CallProjectsApiAsync($"/api/analytics/builder/{userId}/occupied-units");
        return ExtractCount(data);
    }

    public async Task<double> GetOccupancyRateAsync(int userId)
    {
        var data = await CallProjectsApiAsync($"/api/analytics/builder/{userId}/occupancy-rate");
        return ExtractDouble(data);
    }

    public async Task<int> GetMyUnitsCountAsync(int userId)
    {
        var data = await CallProjectsApiAsync($"/api/analytics/owner/{userId}/units-count");
        return ExtractCount(data);
    }

    public async Task<double> GetEnergyEfficiencyAverageAsync(int userId)
    {
        var data = await CallProjectsApiAsync($"/api/analytics/builder/{userId}/energy-efficiency");
        return ExtractDouble(data);
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetOccupancyHistoryAsync(int userId)
    {
        return await CallProjectsApiAsync($"/api/analytics/builder/{userId}/occupancy-history");
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetOwnerUnitsDetailsAsync(int userId)
    {
        return await CallProjectsApiAsync($"/api/analytics/owner/{userId}/units-details");
    }

    private async Task<IEnumerable<Dictionary<string, object>>> CallProjectsApiAsync(string path)
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
            _logger.LogError(ex, "Error calling Projects API at {Path}", path);
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
