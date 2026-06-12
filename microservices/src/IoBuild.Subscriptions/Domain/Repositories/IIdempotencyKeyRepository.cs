using IoBuild.Subscriptions.Domain.Model.Entities;

namespace IoBuild.Subscriptions.Domain.Repositories;

public interface IIdempotencyKeyRepository
{
    Task<bool> ExistsAsync(string key);
    Task AddAsync(IdempotencyKey key);
}
