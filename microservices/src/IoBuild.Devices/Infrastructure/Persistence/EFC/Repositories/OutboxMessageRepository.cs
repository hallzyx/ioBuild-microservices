using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOutboxMessageRepository"/> for the Devices service (ADR-8b).
/// Mirrors IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories.OutboxMessageRepository.
/// </summary>
public class OutboxMessageRepository(DevicesDbContext context) : IOutboxMessageRepository
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

    public async Task UpdateAsync(OutboxMessage message)
    {
        context.OutboxMessages.Update(message);
        await context.SaveChangesAsync();
    }
}
