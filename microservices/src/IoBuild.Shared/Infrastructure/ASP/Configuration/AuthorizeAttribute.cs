using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IoBuild.Shared.Infrastructure.ASP.Configuration;

/// <summary>
/// Custom Authorize attribute.
/// Checks if HttpContext.Items["User"] was set by JwtAuthenticationMiddleware.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.Items["User"];
        if (user is null)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Unauthorized." });
        }
    }
}