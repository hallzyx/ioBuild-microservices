using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SubscriptionsDbContext _context;

    public SubscriptionRepository(SubscriptionsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Subscription entity)
    {
        await _context.Subscriptions.AddAsync(entity);
    }

    public async Task<Subscription?> FindByIdAsync(int id)
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public void Update(Subscription entity)
    {
        _context.Subscriptions.Update(entity);
    }

    public void Remove(Subscription entity)
    {
        _context.Subscriptions.Remove(entity);
    }

    public async Task<IEnumerable<Subscription>> ListAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> FindByBuilderIdAsync(int builderId)
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.BuilderId == builderId)
            .ToListAsync();
    }

    public async Task<Subscription?> FindByPlanIdAsync(int planId)
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.PlanId == planId);
    }
}
