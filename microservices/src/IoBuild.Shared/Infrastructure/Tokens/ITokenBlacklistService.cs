using Microsoft.Extensions.Caching.Memory;

namespace IoBuild.Shared.Infrastructure.Tokens;

/// <summary>
/// Global JWT token blacklist for instant revocation.
/// Addresses QA-1: "Revocation strategy for compromised tokens."
/// Uses IMemoryCache with token's natural expiration as cache TTL.
/// In production, replace with Redis for distributed invalidation.
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Revokes a token (adds it to the blacklist).
    /// The token remains blacklisted until its natural expiration.
    /// </summary>
    /// <param name="token">The JWT to revoke</param>
    /// <param name="expiration">Token's expiration DateTime</param>
    void RevokeToken(string token, DateTime expiration);

    /// <summary>
    /// Checks if a token has been revoked.
    /// </summary>
    /// <param name="token">The JWT to check</param>
    /// <returns>true if the token is blacklisted</returns>
    bool IsTokenRevoked(string token);
}

/// <summary>
/// In-memory implementation using IMemoryCache.
/// Tokens remain cached until their natural expiration or absolute max of 1 hour.
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _cache;

    public TokenBlacklistService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void RevokeToken(string token, DateTime expiration)
    {
        var cacheKey = $"blacklist:{token.GetHashCode():X}";

        // Cache duration = time until token naturally expires
        var remaining = expiration - DateTime.UtcNow;

        // Cap at token's remaining validity (minimum 1 minute, maximum 7 days)
        var cacheDuration = remaining.TotalSeconds switch
        {
            < 60 => TimeSpan.FromMinutes(1),
            > 604800 => TimeSpan.FromDays(7), // 7 days in seconds
            _ => remaining
        };

        _cache.Set(cacheKey, true, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheDuration,
            Priority = CacheItemPriority.Normal
        });
    }

    public bool IsTokenRevoked(string token)
    {
        var cacheKey = $"blacklist:{token.GetHashCode():X}";
        return _cache.TryGetValue(cacheKey, out _);
    }
}
