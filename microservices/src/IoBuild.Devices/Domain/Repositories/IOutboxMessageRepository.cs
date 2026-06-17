using IoBuild.Devices.Domain.Model.Entities;

namespace IoBuild.Devices.Domain.Repositories;

/// <summary>
/// Repository contract for the Devices outbox table (ADR-8b).
/// Mirrors IoBuild.Subscriptions.Domain.Repositories.IOutboxMessageRepository.
/// </summary>
public interface IOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetPendingAsync();
    Task AddAsync(OutboxMessage message);
    Task UpdateAsync(OutboxMessage message);
}
