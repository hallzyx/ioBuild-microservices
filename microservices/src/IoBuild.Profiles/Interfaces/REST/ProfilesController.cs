using System.Net.Mime;
using IoBuild.Profiles.Domain.Model.Queries;
using IoBuild.Profiles.Domain.Services;
using IoBuild.Profiles.Interfaces.REST.Resources;
using IoBuild.Profiles.Interfaces.REST.Transform;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Profiles.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ProfilesController(
    IProfileCommandService profileCommandService,
    IProfileQueryService profileQueryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProfileResource), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileResource resource)
    {
        var createProfileCommand = CreateProfileCommandFromResourceAssembler.ToCommandFromResource(resource);
        var profile = await profileCommandService.Handle(createProfileCommand);
        if (profile is null) return BadRequest();
        var profileResource = ProfileResourceFromEntityAssembler.ToResourceFromEntity(profile);
        return StatusCode(201, profileResource);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProfileResource>), 200)]
    public async Task<IActionResult> GetAllProfiles([FromQuery] int? userId)
    {
        if (userId.HasValue)
        {
            var getByUserIdQuery = new GetProfileByUserIdQuery(userId.Value);
            var profile = await profileQueryService.Handle(getByUserIdQuery);
            if (profile is null) return Ok(Array.Empty<ProfileResource>());
            return Ok(new[] { ProfileResourceFromEntityAssembler.ToResourceFromEntity(profile) });
        }
        var getAllProfilesQuery = new GetAllProfilesQuery();
        var profiles = await profileQueryService.Handle(getAllProfilesQuery);
        var profileResources = profiles.Select(ProfileResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(profileResources);
    }

    [HttpPut("{profileId:int}")]
    [ProducesResponseType(typeof(ProfileResource), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProfile(int profileId, [FromBody] UpdateProfileResource resource)
    {
        var updateProfileCommand = UpdateProfileCommandFromResourceAssembler.ToCommandFromResource(resource, profileId);
        var profile = await profileCommandService.Handle(updateProfileCommand);
        if (profile is null) return BadRequest();
        var profileResource = ProfileResourceFromEntityAssembler.ToResourceFromEntity(profile);
        return Ok(profileResource);
    }
}
