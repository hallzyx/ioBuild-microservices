using IoBuild.IAM.Application.Internal.OutboundServices;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using IoBuild.Shared.Infrastructure.Tokens;

namespace IoBuild.IAM.Infrastructure.Pipeline.Middleware.Components;

public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITokenService tokenService,
        IUserRepository userRepository,
        ITokenBlacklistService blacklistService)
    {
        var endpoint = context.GetEndpoint();

        // Si no hay endpoint (health checks, archivos estaticos), pasar
        if (endpoint is null)
        {
            await next(context);
            return;
        }

        // Saltar rutas de salud y swagger (no requieren autenticacion)
        var path = context.Request.Path.Value ?? "";
        if (path.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null)
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader is null || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Authorization token is required." });
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();

        // ── JWT Revocation Check (QA-1) ──
        if (blacklistService.IsTokenRevoked(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Token has been revoked." });
            return;
        }

        var userId = await tokenService.ValidateToken(token);

        if (userId is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token." });
            return;
        }

        var user = await userRepository.FindByIdAsync(userId.Value);
        if (user is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "User not found." });
            return;
        }

        context.Items["User"] = user;
        await next(context);
    }
}
