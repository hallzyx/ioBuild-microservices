using IoBuild.Subscriptions.Domain.Model.Entities;
using IoBuild.Subscriptions.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class IdempotencyKeyRepository : IIdempotencyKeyRepository
{
    private readonly SubscriptionsDbContext _context;

    public IdempotencyKeyRepository(SubscriptionsDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _context.IdempotencyKeys.AnyAsync(k => k.Key == key);
    }

    public async Task AddAsync(IdempotencyKey key)
    {
        await _context.IdempotencyKeys.AddAsync(key);
    }
}
