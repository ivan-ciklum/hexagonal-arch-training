using ClubExample.Adapter.Pulsar.Configuration;
using ClubExample.Adapter.Pulsar.Publishers;
using ClubExample.Core.OutputPorts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClubExample.Adapter.Pulsar;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Apache Pulsar message publisher as a singleton service.
    /// Singleton is appropriate because the Pulsar client manages connection pooling internally.
    /// </summary>
    public static IServiceCollection AddPulsarAdapter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind configuration from appsettings.json - correct syntax
        services.Configure<PulsarOptions>(options =>
            configuration.GetSection(PulsarOptions.SectionName).Bind(options));

        // Register the message publisher as a singleton
        // Singleton because:
        // 1. Pulsar client is thread-safe and manages connection pooling
        // 2. Avoids overhead of recreating connections
        // 3. Producers are created per-message and disposed properly
        services.AddSingleton<IMessagePublisher, PulsarMessagePublisher>();

        return services;
    }

    /// <summary>
    /// Registers Pulsar adapter with explicit service URL for testing/development scenarios.
    /// </summary>
    public static IServiceCollection AddPulsarAdapter(
        this IServiceCollection services,
        string serviceUrl)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceUrl);

        services.Configure<PulsarOptions>(options =>
        {
            options.ServiceUrl = serviceUrl;
        });

        services.AddSingleton<IMessagePublisher, PulsarMessagePublisher>();

        return services;
    }
}
