using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Queries.Clients;

namespace IoBuild.Projects.Application.Services;

public class ClientQueryService : IClientQueryService
{
    private readonly IClientRepository _repository;

    public ClientQueryService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Client>> Handle(GetAllClientsQuery query)
    {
        return await _repository.ListAsync();
    }

    public async Task<Client?> Handle(GetClientByIdQuery query)
    {
        return await _repository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Client>> Handle(GetClientsByProjectIdQuery query)
    {
        return await _repository.FindByProjectIdAsync(query.ProjectId);
    }
}
