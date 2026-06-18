using IoBuild.IAM.Domain.Model.Entities;

namespace IoBuild.IAM.Domain.Repositories;

/// <summary>
/// Repository contract for the IAM outbox table (ADR-8b).
/// Mirrors IoBuild.Projects.Domain.Repositories.IOutboxMessageRepository.
/// </summary>
public interface IIamOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetPendingAsync();
    Task AddAsync(OutboxMessage message);
    Task UpdateAsync(OutboxMessage message);
}
