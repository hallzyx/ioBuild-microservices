namespace IoBuild.Projects.Domain.Services.Queries.Clients;

public class GetClientsByProjectIdQuery
{
    public int ProjectId { get; }

    public GetClientsByProjectIdQuery(int projectId)
    {
        ProjectId = projectId;
    }
}
