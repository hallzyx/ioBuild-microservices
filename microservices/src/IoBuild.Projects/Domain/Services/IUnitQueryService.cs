using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Services.Queries.Units;

namespace IoBuild.Projects.Domain.Services;

public interface IUnitQueryService
{
    Task<IEnumerable<Unit>> Handle(GetAllUnitsQuery query);
    Task<Unit?> Handle(GetUnitByIdQuery query);
}
