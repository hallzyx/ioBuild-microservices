using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IoBuild.Shared.Infrastructure.Middleware;

/// <summary>
/// JWT Authentication middleware for all microservices.
/// Validates JWT tokens from the Authorization header and sets HttpContext.Items["User"].
/// Skips endpoints decorated with [AllowAnonymous] and paths /health, /swagger.
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<TokenSettings> tokenSettings)
    {
        var endpoint = context.GetEndpoint();

        // Skip health & swagger paths
        var path = context.Request.Path.Value ?? "";
        if (path.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Skip if endpoint has [AllowAnonymous]
        if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null)
        {
            await _next(context);
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

        try
        {
            var settings = tokenSettings.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            // Extract basic user info from claims and store in HttpContext
            var userId = principal.FindFirst(ClaimTypes.Sid)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (userId is null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid token claims." });
                return;
            }

            // Store authenticated user info
            context.Items["UserId"] = int.Parse(userId);
            context.Items["UserEmail"] = email;
            context.Items["UserRole"] = role;
            context.Items["User"] = new { Id = int.Parse(userId), Email = email, Role = role };

            await _next(context);
        }
        catch (SecurityTokenExpiredException)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Token has expired." });
        }
        catch (Exception)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token." });
        }
    }
}

/// <summary>
/// Extension method to register JwtAuthenticationMiddleware in the pipeline.
/// </summary>
public static class JwtAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtAuthenticationMiddleware>();
    }
}