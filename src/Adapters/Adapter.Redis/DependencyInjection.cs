using ClubExample.Adapter.Redis.Repositories;
using ClubExample.Core.OutputPorts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClubExample.Adapter.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure StackExchange.Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException(
                    "Redis connection string not found. Configure 'ConnectionStrings:Redis' in appsettings.json");
            
            // Optional: configure instance name to prefix all keys (useful in shared Redis instances)
            options.InstanceName = configuration["Cache:Redis:InstanceName"] ?? "ClubApp_";
        });

        // Register our cache port implementation
        services.AddScoped<ICacheRepository, RedisCacheRepository>();

        return services;
    }
    
    public static IServiceCollection AddInMemoryCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheRepository, RedisCacheRepository>();
        
        return services;
    }
}
