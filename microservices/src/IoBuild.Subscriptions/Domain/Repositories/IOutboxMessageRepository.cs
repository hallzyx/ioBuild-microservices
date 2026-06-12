using IoBuild.Subscriptions.Domain.Model.Entities;

namespace IoBuild.Subscriptions.Domain.Repositories;

public interface IOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetPendingAsync();
    Task AddAsync(OutboxMessage message);
    Task UpdateAsync(OutboxMessage message);
}
