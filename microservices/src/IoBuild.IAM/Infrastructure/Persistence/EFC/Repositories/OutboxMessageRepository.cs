using IoBuild.IAM.Domain.Model.Entities;
using IoBuild.IAM.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.IAM.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IIamOutboxMessageRepository"/> for the IAM service (ADR-8b).
/// Mirrors IoBuild.Projects.Infrastructure.Repositories.OutboxMessageRepository.
/// </summary>
public class OutboxMessageRepository(ApplicationDbContext context) : IIamOutboxMessageRepository
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
