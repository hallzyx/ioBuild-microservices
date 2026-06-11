using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Profiles.Domain.Model.Queries;
using IoBuild.Profiles.Domain.Repositories;
using IoBuild.Profiles.Domain.Services;

namespace IoBuild.Profiles.Application.Internal.QueryServices;

public class ProfileQueryService(IProfileRepository profileRepository) : IProfileQueryService
{
    public async Task<IEnumerable<Profile>> Handle(GetAllProfilesQuery query)
    {
        return await profileRepository.ListAsync();
    }

    public async Task<Profile?> Handle(GetProfileByIdQuery query)
    {
        return await profileRepository.FindByIdAsync(query.ProfileId);
    }

    public async Task<Profile?> Handle(GetProfileByUserIdQuery query)
    {
        return await profileRepository.FindByUserIdAsync(query.UserId);
    }
}
