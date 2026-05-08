using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Clients;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.Projects.Application.Services;

public class ClientCommandService : IClientCommandService
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientCommandService(IClientRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateClientCommand command)
    {
        var client = new Client(
            command.FullName,
            command.ProjectId,
            command.ProjectName,
            command.Email,
            command.PhoneNumber,
            command.Address);

        await _repository.AddAsync(client);
        await _unitOfWork.CompleteAsync();
        return client.Id;
    }

    public async Task Handle(UpdateClientCommand command)
    {
        var client = await _repository.FindByIdAsync(command.Id);
        if (client == null)
            throw new KeyNotFoundException($"Client with id {command.Id} not found.");

        client.Update(
            command.FullName,
            command.ProjectName,
            command.AccountStatement,
            command.Email,
            command.PhoneNumber,
            command.Address);

        _repository.Update(client);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(DeleteClientCommand command)
    {
        var client = await _repository.FindByIdAsync(command.Id);
        if (client == null)
            throw new KeyNotFoundException($"Client with id {command.Id} not found.");

        _repository.Remove(client);
        await _unitOfWork.CompleteAsync();
    }
}
