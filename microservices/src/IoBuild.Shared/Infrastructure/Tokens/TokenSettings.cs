namespace IoBuild.Shared.Infrastructure.Tokens;

/// <summary>
/// JWT Token settings configuration.
/// Bound from appsettings.json section "TokenSettings".
/// </summary>
public class TokenSettings
{
    public string Secret { get; set; } = string.Empty;
}