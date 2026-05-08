using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Interfaces.Resources;

namespace IoBuild.Projects.Interfaces.Transform;

public class ProjectResourceFromEntityAssembler
{
    public static ProjectResource ToResource(Project entity)
    {
        return new ProjectResource
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Location = entity.Location,
            TotalUnits = entity.TotalUnits,
            OccupiedUnits = entity.OccupiedUnits,
            Status = entity.Status,
            BuilderId = entity.BuilderId,
            CreatedDate = entity.CreatedDate,
            ImageUrl = entity.ImageUrl
        };
    }

    public static IEnumerable<ProjectResource> ToResourceEnumerable(IEnumerable<Project> entities)
    {
        return entities.Select(ToResource);
    }
}
