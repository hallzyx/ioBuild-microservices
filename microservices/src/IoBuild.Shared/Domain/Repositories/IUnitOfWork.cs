namespace IoBuild.Shared.Domain.Repositories;

/// <summary>
/// Unit of Work interface for transactional consistency.
/// </summary>
public interface IUnitOfWork
{
    Task CompleteAsync();
}
