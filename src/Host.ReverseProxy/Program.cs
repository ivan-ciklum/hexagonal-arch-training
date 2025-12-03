using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add YARP Reverse Proxy services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add health checks for monitoring
builder.Services.AddHealthChecks();

var app = builder.Build();

// Development-friendly endpoint to show proxy configuration
app.MapGet("/", () => Results.Ok(new
{
    message = "Club Management API Gateway",
    description = "YARP Reverse Proxy routing traffic to Read and Write APIs",
    environment = app.Environment.EnvironmentName,
    routes = new
    {
        write = new
        {
            path = "/api/write/**",
            target = "Host.Write (Commands + gRPC)",
            methods = new[] { "POST", "PUT", "DELETE" },
            swagger = "/api/write/swagger"
        },
        read = new
        {
            path = "/api/read/**",
            target = "Host.Read (Queries)",
            methods = new[] { "GET" },
            swagger = "/api/read/swagger"
        }
    },
    loadBalancing = new
    {
        writePolicy = builder.Configuration["ReverseProxy:Clusters:write-cluster:LoadBalancingPolicy"] ?? "PowerOfTwoChoices",
        readPolicy = builder.Configuration["ReverseProxy:Clusters:read-cluster:LoadBalancingPolicy"] ?? "PowerOfTwoChoices",
        note = "Check appsettings for current configuration"
    },
    health = "/health",
    examples = new
    {
        writeCommand = "POST /api/write/members",
        readQuery = "GET /api/read/members/expiring?days=30"
    }
}))
.ExcludeFromDescription();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map YARP reverse proxy middleware
app.MapReverseProxy();

app.Run();
