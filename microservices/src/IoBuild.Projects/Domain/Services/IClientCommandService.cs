using IoBuild.Projects.Domain.Services.Commands.Clients;

namespace IoBuild.Projects.Domain.Services;

public interface IClientCommandService
{
    Task<int> Handle(CreateClientCommand command);
    Task Handle(UpdateClientCommand command);
    Task Handle(DeleteClientCommand command);
}
