using IoBuild.IAM.Domain.Services;
using IoBuild.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using IoBuild.IAM.Interfaces.REST.Resources;
using IoBuild.IAM.Interfaces.REST.Transform;
using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.IAM.Interfaces.REST;

[ApiController]
public class AuthenticationController(
    IUserCommandService userCommandService,
    ITokenBlacklistService tokenBlacklistService) : ControllerBase
{
    [HttpPost("api/v1/sessions")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource)
    {
        var command = SignInCommandFromResourceAssembler.ToCommand(resource);
        var (user, token) = await userCommandService.Handle(command);
        var authenticatedUser = AuthenticatedUserResourceFromEntityAssembler.ToResource(user, token);
        return StatusCode(201, authenticatedUser);
    }

    [HttpPost("api/v1/users")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource)
    {
        var command = SignUpCommandFromResourceAssembler.ToCommand(resource);
        await userCommandService.Handle(command);
        return StatusCode(201, new { message = "User created successfully." });
    }

    /// <summary>
    /// Revokes the current user's JWT token.
    /// Implements QA-1: Global JWT revocation via blacklist.
    /// </summary>
    [HttpDelete("api/v1/sessions/current")]
    public async Task<IActionResult> Logout()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
            return BadRequest(new { error = "No token provided." });

        var token = authHeader["Bearer ".Length..].Trim();

        // Revoke the token (blacklist until natural expiration)
        // In production, decode JWT to get actual exp claim
        await tokenBlacklistService.RevokeTokenAsync(token, DateTime.UtcNow.AddDays(7));

        return NoContent();
    }
}
