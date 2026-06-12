using IoBuild.Subscriptions.Domain.Model.Entities;
using IoBuild.Subscriptions.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly SubscriptionsDbContext _context;

    public OutboxMessageRepository(SubscriptionsDbContext context)
    {
        _context = context;
    }

    public async Task<List<OutboxMessage>> GetPendingAsync()
    {
        return await _context.OutboxMessages
            .Where(o => o.Status == "Pending")
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(OutboxMessage message)
    {
        await _context.OutboxMessages.AddAsync(message);
    }

    public async Task UpdateAsync(OutboxMessage message)
    {
        _context.OutboxMessages.Update(message);
        await Task.CompletedTask;
    }
}
