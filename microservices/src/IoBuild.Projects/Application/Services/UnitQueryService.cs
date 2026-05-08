using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Queries.Units;

namespace IoBuild.Projects.Application.Services;

public class UnitQueryService : IUnitQueryService
{
    private readonly IUnitRepository _repository;

    public UnitQueryService(IUnitRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Unit>> Handle(GetAllUnitsQuery query)
    {
        return await _repository.ListAsync();
    }

    public async Task<Unit?> Handle(GetUnitByIdQuery query)
    {
        return await _repository.FindByIdAsync(query.Id);
    }
}
