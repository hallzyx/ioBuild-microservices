namespace IoBuild.Projects.Domain.Services.Queries.Units;

public class GetUnitByIdQuery
{
    public int Id { get; }

    public GetUnitByIdQuery(int id)
    {
        Id = id;
    }
}
