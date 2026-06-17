using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOutboxMessageRepository"/> for the Projects service (ADR-8b).
/// Mirrors IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories.OutboxMessageRepository.
/// </summary>
public class OutboxMessageRepository(AppDbContext context) : IOutboxMessageRepository
{
    public async Task<List<OutboxMessage>> GetPendingAsync()
    {
        return await context.OutboxMessages
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(OutboxMessage message)
    {
        await context.OutboxMessages.AddAsync(message);
    }

    public Task UpdateAsync(OutboxMessage message)
    {
        context.OutboxMessages.Update(message);
        return Task.CompletedTask;
    }
}
