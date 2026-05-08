using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Units;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Projects.Application.Services;

public class UnitCommandService : IUnitCommandService
{
    private readonly IUnitRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UnitCommandService(IUnitRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateUnitCommand command)
    {
        var unit = new Unit(
            command.ProjectId,
            command.UnitNumber,
            command.OwnerId);

        await _repository.AddAsync(unit);
        await _unitOfWork.CompleteAsync();
        return unit.Id;
    }
}
