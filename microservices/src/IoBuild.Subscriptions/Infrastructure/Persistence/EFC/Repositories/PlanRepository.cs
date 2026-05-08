using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly SubscriptionsDbContext _context;

    public PlanRepository(SubscriptionsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Plan entity)
    {
        await _context.Plans.AddAsync(entity);
    }

    public async Task<Plan?> FindByIdAsync(int id)
    {
        return await _context.Plans.FindAsync(id);
    }

    public void Update(Plan entity)
    {
        _context.Plans.Update(entity);
    }

    public void Remove(Plan entity)
    {
        _context.Plans.Remove(entity);
    }

    public async Task<IEnumerable<Plan>> ListAsync()
    {
        return await _context.Plans.ToListAsync();
    }

    public async Task<Plan?> FindByNameAsync(string name)
    {
        return await _context.Plans.FirstOrDefaultAsync(p => p.Name == name);
    }
}
