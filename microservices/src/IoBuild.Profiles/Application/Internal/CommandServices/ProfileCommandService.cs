using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Profiles.Domain.Model.Commands;
using IoBuild.Profiles.Domain.Repositories;
using IoBuild.Profiles.Domain.Services;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Profiles.Application.Internal.CommandServices;

/// <summary>
/// Profile Command Service
/// </summary>
/// <remarks>
/// This class implements the IProfileCommandService interface.
/// It handles the commands for the Profile aggregate.
/// </remarks>
/// <param name="profileRepository">The <see cref="IProfileRepository"/> instance</param>
/// <param name="unitOfWork">The <see cref="IUnitOfWork"/> instance</param>
public class ProfileCommandService(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
    : IProfileCommandService
{
    /// <inheritdoc />
    public async Task<Profile?> Handle(CreateProfileCommand command)
    {
        // Check if a profile with the same UserId already exists
        var existingProfile = await profileRepository.FindByUserIdAsync(command.UserId);
        if (existingProfile != null)
        {
            throw new Exception($"A profile for UserId {command.UserId} already exists.");
        }

        var profile = new Profile(
            command.UserId,
            command.PhotoUrl,
            command.Name,
            command.Username,
            command.Address,
            command.Age,
            command.PhoneNumber
        );

        try
        {
            await profileRepository.AddAsync(profile);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while creating the profile: {e.Message}");
        }

        return profile;
    }

    /// <inheritdoc />
    public async Task<Profile?> Handle(UpdateProfileCommand command)
    {
        // Check if the profile exists
        var profile = await profileRepository.FindByIdAsync(command.Id);
        if (profile == null)
        {
            throw new Exception($"Profile with Id {command.Id} not found.");
        }

        // Update the profile information
        profile.UpdateInformation(
            command.PhotoUrl,
            command.Name,
            command.Username,
            command.Address,
            command.Age,
            command.PhoneNumber
        );

        try
        {
            profileRepository.Update(profile);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while updating the profile: {e.Message}");
        }

        return profile;
    }
}
