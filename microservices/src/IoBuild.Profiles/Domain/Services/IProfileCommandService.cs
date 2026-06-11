using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Profiles.Domain.Model.Commands;

namespace IoBuild.Profiles.Domain.Services;

/// <summary>
/// Profile Command Service interface
/// </summary>
/// <remarks>
/// This interface defines the contract for the Profile command service.
/// </remarks>
public interface IProfileCommandService
{
    /// <summary>
    /// Handle Create Profile Command
    /// </summary>
    /// <param name="command">The <see cref="CreateProfileCommand"/> command</param>
    /// <returns>The created <see cref="Profile"/> object</returns>
    Task<Profile?> Handle(CreateProfileCommand command);

    /// <summary>
    /// Handle Update Profile Command
    /// </summary>
    /// <param name="command">The <see cref="UpdateProfileCommand"/> command</param>
    /// <returns>The updated <see cref="Profile"/> object</returns>
    Task<Profile?> Handle(UpdateProfileCommand command);
}
