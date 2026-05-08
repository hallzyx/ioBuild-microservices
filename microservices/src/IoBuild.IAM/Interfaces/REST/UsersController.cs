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
    IUserQueryService userQueryService,
    IUserCommandService userCommandService) : ControllerBase
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

    [HttpPut("{userId}/password")]
    public async Task<IActionResult> UpdatePassword(int userId, [FromBody] UpdatePasswordResource resource)
    {
        var command = UpdatePasswordCommandFromResourceAssembler.ToCommand(userId, resource);
        await userCommandService.Handle(command);
        return Ok(new { message = "Password updated successfully." });
    }
}
