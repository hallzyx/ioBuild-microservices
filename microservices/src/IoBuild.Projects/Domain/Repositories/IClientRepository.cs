using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Projects.Domain.Repositories;

public interface IClientRepository : IBaseRepository<Client>
{
    Task<IEnumerable<Client>> FindByProjectIdAsync(int projectId);
}
