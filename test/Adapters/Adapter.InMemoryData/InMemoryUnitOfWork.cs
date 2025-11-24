using ClubExample.Core.OutputPorts;

namespace ClubExample.Adapter.InMemoryData;

/// <summary>
/// In-memory implementation of IUnitOfWork for unit testing.
/// Provides transactional semantics for in-memory collections.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly Dictionary<Type, List<object>> _entities = new();
    private readonly List<(Type Type, object Entity, Operation Operation)> _pendingOperations = new();

    private enum Operation
    {
        Add,
        Update,
        Delete
    }

    public Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        _pendingOperations.Add((typeof(T), entity, Operation.Add));
        return Task.CompletedTask;
    }

    public Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        _pendingOperations.Add((typeof(T), entity, Operation.Update));
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        _pendingOperations.Add((typeof(T), entity, Operation.Delete));
        return Task.CompletedTask;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        int changesCount = 0;

        foreach (var (type, entity, operation) in _pendingOperations)
        {
            if (!_entities.ContainsKey(type))
            {
                _entities[type] = new List<object>();
            }

            var collection = _entities[type];

            switch (operation)
            {
                case Operation.Add:
                    collection.Add(entity);
                    changesCount++;
                    break;

                case Operation.Update:
                    // In memory, entities are already tracked by reference
                    // No need to do anything special
                    changesCount++;
                    break;

                case Operation.Delete:
                    collection.Remove(entity);
                    changesCount++;
                    break;
            }
        }

        _pendingOperations.Clear();
        return Task.FromResult(changesCount);
    }

    /// <summary>
    /// Gets all entities of a specific type (for testing purposes).
    /// </summary>
    public IReadOnlyList<T> GetAll<T>() where T : class
    {
        if (!_entities.ContainsKey(typeof(T)))
        {
            return Array.Empty<T>();
        }

        return _entities[typeof(T)].Cast<T>().ToList().AsReadOnly();
    }

    /// <summary>
    /// Clears all entities (for testing purposes).
    /// </summary>
    public void Clear()
    {
        _entities.Clear();
        _pendingOperations.Clear();
    }
}
