using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Client entity)
    {
        await _context.Clients.AddAsync(entity);
    }

    public async Task<Client?> FindByIdAsync(int id)
    {
        return await _context.Clients.FindAsync(id);
    }

    public void Update(Client entity)
    {
        _context.Clients.Update(entity);
    }

    public void Remove(Client entity)
    {
        _context.Clients.Remove(entity);
    }

    public async Task<IEnumerable<Client>> ListAsync()
    {
        return await _context.Clients.ToListAsync();
    }

    public async Task<IEnumerable<Client>> FindByProjectIdAsync(int projectId)
    {
        return await _context.Clients.Where(c => c.ProjectId == projectId).ToListAsync();
    }
}
