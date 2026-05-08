using IoBuild.IAM.Domain.Services;
using IoBuild.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using IoBuild.IAM.Interfaces.REST.Resources;
using IoBuild.IAM.Interfaces.REST.Transform;
using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.IAM.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthenticationController(
    IUserCommandService userCommandService,
    ITokenBlacklistService tokenBlacklistService) : ControllerBase
{
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource)
    {
        var command = SignInCommandFromResourceAssembler.ToCommand(resource);
        var (user, token) = await userCommandService.Handle(command);
        var authenticatedUser = AuthenticatedUserResourceFromEntityAssembler.ToResource(user, token);
        return Ok(authenticatedUser);
    }

    [HttpPost("sign-up")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource)
    {
        var command = SignUpCommandFromResourceAssembler.ToCommand(resource);
        await userCommandService.Handle(command);
        return Ok(new { message = "User created successfully." });
    }

    /// <summary>
    /// Revokes the current user's JWT token.
    /// Implements QA-1: Global JWT revocation via blacklist.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
            return BadRequest(new { error = "No token provided." });

        var token = authHeader["Bearer ".Length..].Trim();

        // Revoke the token (blacklist until natural expiration)
        // In production, decode JWT to get actual exp claim
        tokenBlacklistService.RevokeToken(token, DateTime.UtcNow.AddDays(7));

        return Ok(new { message = "Logged out successfully. Token revoked." });
    }
}
