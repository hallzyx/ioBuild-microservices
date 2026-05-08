using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Projects.Domain.Repositories;

public interface IProjectRepository : IBaseRepository<Project>
{
}
