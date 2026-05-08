namespace IoBuild.Projects.Domain.Services.Queries.Projects;

public class GetProjectByIdQuery
{
    public int Id { get; }

    public GetProjectByIdQuery(int id)
    {
        Id = id;
    }
}
