namespace IoBuild.Analytics.Interfaces.ACL;

public interface IProjectsContextFacade
{
    Task<IEnumerable<Dictionary<string, object>>> GetProjectsByBuilderAsync(int userId);
    Task<IEnumerable<Dictionary<string, object>>> GetUnitsByOwnerAsync(int userId);
    Task<IEnumerable<Dictionary<string, object>>> GetUnitsByProjectAsync(int projectId);
    Task<int> GetActiveProjectsCountAsync(int userId);
    Task<int> GetTotalUnitsAsync(int userId);
    Task<int> GetOccupiedUnitsAsync(int userId);
    Task<double> GetOccupancyRateAsync(int userId);
    Task<int> GetMyUnitsCountAsync(int userId);
    Task<double> GetEnergyEfficiencyAverageAsync(int userId);
    Task<IEnumerable<Dictionary<string, object>>> GetOccupancyHistoryAsync(int userId);
    Task<IEnumerable<Dictionary<string, object>>> GetOwnerUnitsDetailsAsync(int userId);
}
