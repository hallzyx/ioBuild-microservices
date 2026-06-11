using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Profiles.Domain.Model.Queries;

namespace IoBuild.Profiles.Domain.Services;

public interface IProfileQueryService
{
    Task<IEnumerable<Profile>> Handle(GetAllProfilesQuery query);
    Task<Profile?> Handle(GetProfileByIdQuery query);
    Task<Profile?> Handle(GetProfileByUserIdQuery query);
}
