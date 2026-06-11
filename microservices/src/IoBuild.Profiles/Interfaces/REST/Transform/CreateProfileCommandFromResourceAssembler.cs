using IoBuild.Profiles.Domain.Model.Commands;
using IoBuild.Profiles.Interfaces.REST.Resources;

namespace IoBuild.Profiles.Interfaces.REST.Transform;

public static class CreateProfileCommandFromResourceAssembler
{
    public static CreateProfileCommand ToCommandFromResource(CreateProfileResource resource)
    {
        return new CreateProfileCommand(
            resource.UserId,
            resource.PhotoUrl,
            resource.Name,
            resource.Username,
            resource.Address,
            resource.Age,
            resource.PhoneNumber
        );
    }
}
