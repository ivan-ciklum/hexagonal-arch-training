using ClubExample.Adapter.Api.Endpoints;
using ClubExample.Adapter.gRPC.Services;
using ClubExample.Adapter.PostgreSQL;
using ClubExample.Adapter.PostgreSQL.Repositories;
using ClubExample.Adapter.Pulsar;
using ClubExample.Adapter.Redis;
using ClubExample.Core.InputPorts;
using ClubExample.Core.OutputPorts;
using ClubExample.Core.UseCases;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Enable HTTP/2 without TLS in development (required for gRPC)
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        // HTTP endpoint (gRPC uses HTTP/2)
        options.ListenLocalhost(5000, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
        
        // HTTPS endpoint (REST + gRPC with TLS)
        options.ListenLocalhost(5001, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            listenOptions.UseHttps();
        });
    });
}

// Add gRPC services with reflection (for development/debugging)
builder.Services.AddGrpc();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Club Management API",
        Version = "v1",
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
    
    options.UseNpgsql(connectionString);
});

// Register Unit of Work - Centralizes transaction control
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ClubDbContext>());

// Register Output Ports (Repositories) - Driven Adapters
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();

// Register Redis Cache Adapter - Distributed cache infrastructure
builder.Services.AddRedisCache(builder.Configuration);

// Register Pulsar Messaging Adapter - Event-driven communication
builder.Services.AddPulsarAdapter(builder.Configuration);

// Register Input Ports (Use Cases) - Core business logic
builder.Services.AddScoped<IRegisterMemberUseCase, RegisterMemberUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Club Management API v1");
        options.RoutePrefix = "swagger"; // Swagger UI at /swagger
        options.DocumentTitle = "Club Management API";
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

// Root endpoint - redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

// Register REST API endpoints from Adapter.Api
app.MapMemberEndpoints();

// Register gRPC services from Adapter.gRPC
app.MapGrpcService<MemberGrpcService>();

// Map gRPC reflection service in development
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
    
    app.MapGet("/grpc", () => Results.Ok(new
    {
        message = "gRPC endpoint is available",
        service = "MemberService",
        methods = new[] { "RegisterMember" },
        http2_endpoint = "http://127.0.0.1:5000",
        https_endpoint = "https://127.0.0.1:5001",
        reflection = "Enabled for grpcurl",
        usage = "grpcurl -plaintext 127.0.0.1:5000 list",
        swagger_ui = "https://127.0.0.1:5001/swagger"
    }))
    .WithTags("Info");
}

app.Run();
