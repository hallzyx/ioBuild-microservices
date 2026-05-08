using IoBuild.Projects.Domain.Services.Commands.Units;

namespace IoBuild.Projects.Domain.Services;

public interface IUnitCommandService
{
    Task<int> Handle(CreateUnitCommand command);
}
