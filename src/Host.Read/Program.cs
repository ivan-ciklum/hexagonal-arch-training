using ClubExample.Adapter.Api.Endpoints;
using ClubExample.Adapter.PostgreSQL;
using ClubExample.Adapter.PostgreSQL.Repositories;
using ClubExample.Adapter.Redis;
using ClubExample.Core.InputPorts;
using ClubExample.Core.OutputPorts;
using ClubExample.Core.UseCases;
using ClubExample.Host.Read.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry instrumentation - Must be early in the pipeline
builder.Services.AddOpenTelemetryInstrumentation(builder.Configuration, builder.Environment);

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Club Management Read API",
        Version = "v1",
        Description = "Query-only API for read operations (CQRS Read Side)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Club Management Team"
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Try to include Adapter.Api XML comments
    var adapterXmlPath = Path.Combine(AppContext.BaseDirectory, "Adapter.Api.xml");
    if (File.Exists(adapterXmlPath))
    {
        options.IncludeXmlComments(adapterXmlPath);
    }
});

// Configure DbContext (PostgreSQL) - Infrastructure concern
builder.Services.AddDbContext<ClubDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ClubDatabase")
        ?? "Host=localhost;Database=club_db;Username=postgres;Password=postgres";
    
    // Read-only configuration with NoTracking for better performance
    options.UseNpgsql(connectionString)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// Register Output Ports (Repositories) - Driven Adapters - READ ONLY
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();

// Register Redis Cache Adapter - Distributed cache infrastructure
builder.Services.AddRedisCache(builder.Configuration);

// Register Input Ports (Use Cases) - Core business logic - QUERIES ONLY
builder.Services.AddScoped<IGetMembersExpiringUseCase, GetMembersExpiringUseCase>();

// Add health checks for monitoring and load balancer probes
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Club Management Read API v1");
        options.RoutePrefix = "swagger"; // Swagger UI at /swagger
        options.DocumentTitle = "Club Management Read API";
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

// Root endpoint - redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

// Health check endpoint for load balancers and monitoring
app.MapHealthChecks("/health");

// Register REST API endpoints from Adapter.Api - QUERIES ONLY
app.MapMemberQueryEndpoints();

app.Run();
