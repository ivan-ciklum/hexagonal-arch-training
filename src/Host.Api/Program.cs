using ClubExample.Adapter.Api.Endpoints;
using ClubExample.Adapter.PostgreSQL;
using ClubExample.Adapter.PostgreSQL.Repositories;
using ClubExample.Core.InputPorts;
using ClubExample.Core.OutputPorts;
using ClubExample.Core.UseCases;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI/Swagger support
builder.Services.AddOpenApi();

// Configure DbContext (PostgreSQL) - Infrastructure concern
builder.Services.AddDbContext<ClubDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ClubDatabase")
        ?? "Host=localhost;Database=club_db;Username=postgres;Password=postgres";
    
    options.UseNpgsql(connectionString);
});

// Register Unit of Work - Centralizes transaction control
// The DbContext implements IUnitOfWork, so we can use it as both
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ClubDbContext>());

// Register Output Ports (Repositories) - Driven Adapters
// Repositories are now lightweight - they only handle queries
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();

// Register Input Ports (Use Cases) - Core business logic
// The Host wires the use cases with their dependencies
builder.Services.AddScoped<IRegisterMemberUseCase, RegisterMemberUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Root endpoint
app.MapGet("/", () => "Club Management API - Hexagonal Architecture with Unit of Work")
    .ExcludeFromDescription();

// Register member endpoints from Adapter.Api
// The Host tells the API adapter to register its routes
app.MapMemberEndpoints();

app.Run();
