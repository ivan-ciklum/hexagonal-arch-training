using ClubExample.Core.OutputPorts;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ClubExample.Adapter.Redis.Repositories;

public sealed class RedisCacheRepository : ICacheRepository
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public RedisCacheRepository(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cachedData = await _cache.GetStringAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(cachedData, SerializerOptions);
        }
        catch (JsonException)
        {
            // Log this in a real scenario - corrupted cache data
            // For now, treat as cache miss
            await _cache.RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var serializedData = JsonSerializer.Serialize(value, SerializerOptions);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
