using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.IAM.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByEmailAsync(string email);
    bool ExistsByEmail(string email);
}
