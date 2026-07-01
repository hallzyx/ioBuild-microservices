namespace IoBuild.Projects.Domain.Services.Queries.Projects;

public class GetProjectsByBuilderIdQuery
{
    public int BuilderId { get; }

    public GetProjectsByBuilderIdQuery(int builderId)
    {
        BuilderId = builderId;
    }
}
