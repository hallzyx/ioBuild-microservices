namespace IoBuild.Projects.Domain.Services.Queries.Clients;

public class GetClientByIdQuery
{
    public int Id { get; }

    public GetClientByIdQuery(int id)
    {
        Id = id;
    }
}
