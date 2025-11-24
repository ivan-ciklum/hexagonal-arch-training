using ClubExample.Adapter.PostgreSQL.Configurations;
using ClubExample.Core.Domain;
using ClubExample.Core.OutputPorts;
using Microsoft.EntityFrameworkCore;

namespace ClubExample.Adapter.PostgreSQL;

public sealed class ClubDbContext : DbContext, IUnitOfWork
{
    public ClubDbContext(DbContextOptions<ClubDbContext> options) : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Club> Clubs => Set<Club>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfiguration(new MemberConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new ClubConfiguration());
    }

    // IUnitOfWork implementation
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        await Set<T>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }
}
