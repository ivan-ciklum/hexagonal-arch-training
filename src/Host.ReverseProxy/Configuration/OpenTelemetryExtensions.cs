using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ClubExample.Host.ReverseProxy.Configuration;

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

        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "ClubExample.ReverseProxy";
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
                    ["service.side"] = "gateway"
                }))
            .WithTracing(tracing => tracing
                // ASP.NET Core instrumentation (for incoming requests to the proxy)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("http.request.headers.user-agent", httpRequest.Headers.UserAgent.ToString());
                        activity.SetTag("http.request.headers.host", httpRequest.Host.ToString());
                        activity.SetTag("proxy.original_path", httpRequest.Path.ToString());
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                    };
                })
                // HTTP Client instrumentation (for outgoing requests from proxy to backends)
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequestMessage = (activity, httpRequest) =>
                    {
                        activity.SetTag("http.client.request.uri", httpRequest.RequestUri?.ToString());
                        activity.SetTag("proxy.backend", httpRequest.RequestUri?.Host);
                    };
                    options.EnrichWithHttpResponseMessage = (activity, httpResponse) =>
                    {
                        activity.SetTag("http.client.response.status_code", (int)httpResponse.StatusCode);
                    };
                })
                // Add custom source for YARP if needed
                .AddSource("Yarp.ReverseProxy")
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
                .AddMeter("Yarp.ReverseProxy")
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
