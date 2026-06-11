using IoBuild.Profiles.Domain.Model.Commands;
using IoBuild.Profiles.Interfaces.REST.Resources;

namespace IoBuild.Profiles.Interfaces.REST.Transform;

public static class UpdateProfileCommandFromResourceAssembler
{
    public static UpdateProfileCommand ToCommandFromResource(UpdateProfileResource resource, int profileId)
    {
        return new UpdateProfileCommand(
            profileId,
            resource.PhotoUrl,
            resource.Name,
            resource.Username,
            resource.Address,
            resource.Age,
            resource.PhoneNumber);
    }
}
