using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Services.Queries.Clients;

namespace IoBuild.Projects.Domain.Services;

public interface IClientQueryService
{
    Task<IEnumerable<Client>> Handle(GetAllClientsQuery query);
    Task<Client?> Handle(GetClientByIdQuery query);
    Task<IEnumerable<Client>> Handle(GetClientsByProjectIdQuery query);
}
