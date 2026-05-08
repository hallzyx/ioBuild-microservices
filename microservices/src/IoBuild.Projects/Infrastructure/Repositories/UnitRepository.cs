using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly AppDbContext _context;

    public UnitRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Unit entity)
    {
        await _context.Units.AddAsync(entity);
    }

    public async Task<Unit?> FindByIdAsync(int id)
    {
        return await _context.Units.FindAsync(id);
    }

    public void Update(Unit entity)
    {
        _context.Units.Update(entity);
    }

    public void Remove(Unit entity)
    {
        _context.Units.Remove(entity);
    }

    public async Task<IEnumerable<Unit>> ListAsync()
    {
        return await _context.Units.ToListAsync();
    }
}
