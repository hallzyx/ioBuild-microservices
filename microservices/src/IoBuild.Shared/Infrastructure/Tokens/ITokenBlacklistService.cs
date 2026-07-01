using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace IoBuild.Shared.Infrastructure.Tokens;

/// <summary>
/// Global JWT token blacklist for instant revocation (QA-1).
/// Production implementation uses Redis for distributed invalidation
/// so revocations survive restarts and propagate across replicas.
/// </summary>
public interface ITokenBlacklistService
{
    Task RevokeTokenAsync(string token, DateTime expiration);
    Task<bool> IsTokenRevokedAsync(string token);
}

/// <summary>
/// In-memory fallback — single-node only, revocations lost on restart.
/// Used when REDIS_CONNECTION is not configured.
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _cache;

    public TokenBlacklistService(IMemoryCache cache) => _cache = cache;

    public Task RevokeTokenAsync(string token, DateTime expiration)
    {
        var duration = ClampDuration(expiration - DateTime.UtcNow);
        _cache.Set(CacheKey(token), true, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        });
        return Task.CompletedTask;
    }

    public Task<bool> IsTokenRevokedAsync(string token) =>
        Task.FromResult(_cache.TryGetValue(CacheKey(token), out _));

    private static string CacheKey(string token) => $"blacklist:{Sha256(token)}";

    private static TimeSpan ClampDuration(TimeSpan remaining) => remaining.TotalSeconds switch
    {
        < 60 => TimeSpan.FromMinutes(1),
        > 604800 => TimeSpan.FromDays(7),
        _ => remaining
    };

    private static string Sha256(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}

/// <summary>
/// Redis implementation — distributed, survives restarts, propagates to replicas.
/// Used when REDIS_CONNECTION env var is present.
/// </summary>
public class RedisTokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;

    public RedisTokenBlacklistService(IDistributedCache cache) => _cache = cache;

    public Task RevokeTokenAsync(string token, DateTime expiration) =>
        _cache.SetStringAsync(CacheKey(token), "1", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ClampDuration(expiration - DateTime.UtcNow)
        });

    public async Task<bool> IsTokenRevokedAsync(string token) =>
        await _cache.GetStringAsync(CacheKey(token)) is not null;

    private static string CacheKey(string token) => $"blacklist:{Sha256(token)}";

    private static TimeSpan ClampDuration(TimeSpan remaining) => remaining.TotalSeconds switch
    {
        < 60 => TimeSpan.FromMinutes(1),
        > 604800 => TimeSpan.FromDays(7),
        _ => remaining
    };

    private static string Sha256(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}
