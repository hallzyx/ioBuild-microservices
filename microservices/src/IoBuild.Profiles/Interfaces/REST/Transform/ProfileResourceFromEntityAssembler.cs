using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Profiles.Interfaces.REST.Resources;

namespace IoBuild.Profiles.Interfaces.REST.Transform;

public static class ProfileResourceFromEntityAssembler
{
    public static ProfileResource ToResourceFromEntity(Profile entity)
    {
        return new ProfileResource(
            entity.Id, 
            entity.UserId, 
            entity.PhotoUrl, 
            entity.Name, 
            entity.Username, 
            entity.Address, 
            entity.Age, 
            entity.PhoneNumber,
            entity.SecondEmail);
    }
}
