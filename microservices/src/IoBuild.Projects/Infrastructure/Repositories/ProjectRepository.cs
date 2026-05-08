using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Project entity)
    {
        await _context.Projects.AddAsync(entity);
    }

    public async Task<Project?> FindByIdAsync(int id)
    {
        return await _context.Projects.FindAsync(id);
    }

    public void Update(Project entity)
    {
        _context.Projects.Update(entity);
    }

    public void Remove(Project entity)
    {
        _context.Projects.Remove(entity);
    }

    public async Task<IEnumerable<Project>> ListAsync()
    {
        return await _context.Projects.ToListAsync();
    }
}
