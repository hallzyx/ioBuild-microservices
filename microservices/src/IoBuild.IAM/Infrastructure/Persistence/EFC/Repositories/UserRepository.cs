using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.IAM.Infrastructure.Persistence.EFC.Repositories;

public class UserRepository(IBaseRepository<User> baseRepository) : IUserRepository
{
    public async Task AddAsync(User entity) => await baseRepository.AddAsync(entity);
    public async Task<User?> FindByIdAsync(int id) => await baseRepository.FindByIdAsync(id);
    public void Update(User entity) => baseRepository.Update(entity);
    public void Remove(User entity) => baseRepository.Remove(entity);
    public async Task<IEnumerable<User>> ListAsync() => await baseRepository.ListAsync();

    public async Task<User?> FindByEmailAsync(string email)
    {
        var users = await baseRepository.ListAsync();
        return users.FirstOrDefault(u => u.Email == email);
    }

    public bool ExistsByEmail(string email)
    {
        var users = baseRepository.ListAsync().GetAwaiter().GetResult();
        return users.Any(u => u.Email == email);
    }
}
