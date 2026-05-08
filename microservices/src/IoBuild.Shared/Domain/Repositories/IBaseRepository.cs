namespace IoBuild.Shared.Domain.Repositories;

/// <summary>
/// Base repository interface for all repositories across microservices.
/// Implements the Repository Pattern (Proxy).
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IBaseRepository<TEntity>
{
    Task AddAsync(TEntity entity);
    Task<TEntity?> FindByIdAsync(int id);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<IEnumerable<TEntity>> ListAsync();
}
