namespace IoBuild.Shared.Infrastructure.ASP.Configuration;

/// <summary>
/// Custom AllowAnonymous attribute.
/// Marks endpoint methods that should skip JWT authentication.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{
}