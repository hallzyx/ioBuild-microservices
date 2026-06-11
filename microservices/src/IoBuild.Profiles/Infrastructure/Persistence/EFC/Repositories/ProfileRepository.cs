using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Profiles.Domain.Repositories;
using IoBuild.Profiles.Infrastructure.Persistence.EFC;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Profiles.Infrastructure.Persistence.EFC.Repositories;

public class ProfileRepository(ProfilesDbContext context) : IProfileRepository
{
    public async Task AddAsync(Profile entity)
    {
        await context.Profiles.AddAsync(entity);
    }

    public async Task<Profile?> FindByIdAsync(int id)
    {
        return await context.Profiles.FindAsync(id);
    }

    public async Task<Profile?> FindByUserIdAsync(int userId)
    {
        return await context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<IEnumerable<Profile>> FindByUsernameAsync(string username)
    {
        return await context.Profiles.Where(p => p.Username.Contains(username)).ToListAsync();
    }

    public async Task<IEnumerable<Profile>> ListAsync()
    {
        return await context.Profiles.ToListAsync();
    }

    public void Remove(Profile entity)
    {
        context.Profiles.Remove(entity);
    }

    public void Update(Profile entity)
    {
        context.Profiles.Update(entity);
    }
}
