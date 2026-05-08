using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class UnitResourceFromEntityAssembler
{
    public static UnitResource ToResource(Unit entity)
    {
        return new UnitResource
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            UnitNumber = entity.UnitNumber,
            OwnerId = entity.OwnerId
        };
    }

    public static IEnumerable<UnitResource> ToResourceEnumerable(IEnumerable<Unit> entities)
    {
        return entities.Select(ToResource);
    }
}
