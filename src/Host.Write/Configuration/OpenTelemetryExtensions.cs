using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ClubExample.Host.Write.Configuration;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryInstrumentation(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "ClubExample.Write";
        var serviceVersion = configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
        var instanceName = configuration["INSTANCE_NAME"] ?? Environment.MachineName;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: instanceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName,
                    ["service.side"] = "write"
                }))
            .WithTracing(tracing => tracing
                // ASP.NET Core instrumentation
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("http.request.headers.user-agent", httpRequest.Headers.UserAgent.ToString());
                        activity.SetTag("http.request.headers.host", httpRequest.Host.ToString());
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                    };
                })
                // HTTP Client instrumentation
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequestMessage = (activity, httpRequest) =>
                    {
                        activity.SetTag("http.client.request.uri", httpRequest.RequestUri?.ToString());
                    };
                })
                // Entity Framework Core instrumentation
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                    options.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.operation", command.CommandType.ToString());
                    };
                })
                // PostgreSQL instrumentation (Npgsql)
                .AddNpgsql()
                // Redis instrumentation
                .AddRedisInstrumentation(options =>
                {
                    options.SetVerboseDatabaseStatements = true;
                    options.EnrichActivityWithTimingEvents = true;
                })
                // gRPC Client instrumentation
                .AddGrpcClientInstrumentation(options =>
                {
                    options.SuppressDownstreamInstrumentation = false;
                })
                // Add custom source for Core/Adapters
                .AddSource("ClubExample.Core")
                .AddSource("ClubExample.Adapter.Pulsar")
                .AddSource("ClubExample.Adapter.PostgreSQL")
                .AddSource("ClubExample.Adapter.Redis")
                // OTLP Exporter
                .AddOtlpExporter(options =>
                {
                    var endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] 
                        ?? configuration["OpenTelemetry:Otlp:Endpoint"]
                        ?? "http://localhost:18889";
                    
                    options.Endpoint = new Uri(endpoint);
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter("ClubExample.Core")
                .AddMeter("ClubExample.Adapter.*")
                .AddOtlpExporter(options =>
                {
                    var endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] 
                        ?? configuration["OpenTelemetry:Otlp:Endpoint"]
                        ?? "http://localhost:18889";
                    
                    options.Endpoint = new Uri(endpoint);
                }));

        return services;
    }
}
