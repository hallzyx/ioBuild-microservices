namespace IoBuild.Projects.Domain.Services.Queries.Units;

public class GetUnitsByProjectIdQuery
{
    public int ProjectId { get; }

    public GetUnitsByProjectIdQuery(int projectId)
    {
        ProjectId = projectId;
    }
}
