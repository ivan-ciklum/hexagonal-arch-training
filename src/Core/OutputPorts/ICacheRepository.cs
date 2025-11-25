namespace ClubExample.Core.OutputPorts;

public interface ICacheRepository
{
    /// <summary>
    /// Retrieves a value from the cache by its key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The unique cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value if found and valid, null otherwise</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value in the cache with an optional expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache</typeparam>
    /// <param name="key">The unique cache key</param>
    /// <param name="value">The value to store</param>
    /// <param name="expiration">Optional expiration duration. If null, uses default policy.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
