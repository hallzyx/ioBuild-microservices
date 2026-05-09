using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IoBuild.IAM.Application.Internal.OutboundServices;
using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IoBuild.IAM.Infrastructure.Tokens.JWT.Services;

public class TokenService(IOptions<TokenSettings> tokenSettings) : ITokenService
{
    private readonly TokenSettings _settings = tokenSettings.Value;

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task<int?> ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
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

            var sidClaim = principal.FindFirst(ClaimTypes.Sid)?.Value;
            if (sidClaim is not null && int.TryParse(sidClaim, out var userId))
                return Task.FromResult<int?>(userId);
        }
        catch
        {
            // Token validation failed
        }

        return Task.FromResult<int?>(null);
    }
}
