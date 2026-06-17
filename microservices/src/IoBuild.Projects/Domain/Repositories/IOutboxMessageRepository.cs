using IoBuild.Projects.Domain.Model.Entities;

namespace IoBuild.Projects.Domain.Repositories;

/// <summary>
/// Repository contract for the Projects outbox table (ADR-8b).
/// Mirrors IoBuild.Devices.Domain.Repositories.IOutboxMessageRepository.
/// </summary>
public interface IOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetPendingAsync();
    Task AddAsync(OutboxMessage message);
    Task UpdateAsync(OutboxMessage message);
}
