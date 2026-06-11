using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Profiles.Domain.Repositories;

public interface IProfileRepository : IBaseRepository<Profile>
{
    Task<Profile?> FindByUserIdAsync(int userId);
    Task<IEnumerable<Profile>> FindByUsernameAsync(string username);
}
