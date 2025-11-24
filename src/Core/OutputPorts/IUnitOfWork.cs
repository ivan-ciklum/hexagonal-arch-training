namespace ClubExample.Core.OutputPorts;

public interface IUnitOfWork
{
    /// <summary>
    /// Marks an entity to be added to the database.
    /// </summary>
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Marks an entity to be updated in the database.
    /// </summary>
    Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Marks an entity to be deleted from the database.
    /// </summary>
    Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Commits all pending changes to the database as a single transaction.
    /// Returns the number of state entries written to the database.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
