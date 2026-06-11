using IoBuild.Profiles.Domain.Model.Queries;
using IoBuild.Profiles.Domain.Services;
using IoBuild.Profiles.Interfaces.ACL;
using IoBuild.Profiles.Interfaces.REST.Resources;
using IoBuild.Profiles.Interfaces.REST.Transform;
using IoBuild.Profiles.Domain.Repositories;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Profiles.Application.ACL;

/// <summary>
/// Facade for the profiles context
/// </summary>
/// <param name="profileQueryService">
/// The profile query service
/// </param>
public class ProfilesContextFacade(
    IProfileQueryService profileQueryService,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork
) : IProfilesContextFacade
{
    // inheritedDoc
    public async Task<ProfileResource?> FetchProfileByUserId(int userId)
    {
        var getProfileByUserIdQuery = new GetProfileByUserIdQuery(userId);
        var profile = await profileQueryService.Handle(getProfileByUserIdQuery);
        return profile == null ? null : ProfileResourceFromEntityAssembler.ToResourceFromEntity(profile);
    }

    // inheritedDoc
    public async Task<string?> FetchSecondEmailByUserId(int userId)
    {
        var profile = await profileRepository.FindByUserIdAsync(userId);
        return profile?.SecondEmail;
    }

    // inheritedDoc
    public async Task SetSecondEmailByUserId(int userId, string? secondEmail)
    {
        var profile = await profileRepository.FindByUserIdAsync(userId);
        if (profile is null) throw new Exception($"Profile not found for UserId {userId}");
        profile.UpdateSecondEmail(secondEmail);
        await unitOfWork.CompleteAsync();
    }
}
