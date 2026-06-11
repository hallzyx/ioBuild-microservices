using IoBuild.Profiles.Interfaces.REST.Resources;

namespace IoBuild.Profiles.Interfaces.ACL;

/// <summary>
/// Facade for the profiles context
/// </summary>
public interface IProfilesContextFacade
{
    /// <summary>
    /// Fetch profile by user id
    /// </summary>
    /// <param name="userId">
    /// User id of the profile to fetch
    /// </param>
    /// <returns>
    /// The profile resource if found, null otherwise
    /// </returns>
    Task<ProfileResource?> FetchProfileByUserId(int userId);
    
    /// <summary>
    /// Fetch the second email for a profile by user id
    /// </summary>
    Task<string?> FetchSecondEmailByUserId(int userId);

    /// <summary>
    /// Set or update the second email for a profile by user id
    /// </summary>
    Task SetSecondEmailByUserId(int userId, string? secondEmail);
}
