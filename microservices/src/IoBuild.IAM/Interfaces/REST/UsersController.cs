using IoBuild.IAM.Domain.Services;
using IoBuild.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using IoBuild.IAM.Interfaces.REST.Resources;
using IoBuild.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.IAM.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController(
    IUserQueryService userQueryService) : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var query = new GetUserByIdQuery(userId);
        var user = await userQueryService.Handle(query);
        if (user is null)
            return NotFound(new { error = "User not found." });

        var resource = UserResourceFromEntityAssembler.ToResource(user);
        return Ok(resource);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var users = await userQueryService.Handle(query);
        var resources = UserResourceFromEntityAssembler.ToResourceList(users);
        return Ok(resources);
    }
}
