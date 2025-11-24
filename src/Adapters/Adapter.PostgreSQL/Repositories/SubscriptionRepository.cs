using ClubExample.Core.Domain;
using ClubExample.Core.OutputPorts;
using Microsoft.EntityFrameworkCore;

namespace ClubExample.Adapter.PostgreSQL.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ClubDbContext _context;

    public SubscriptionRepository(ClubDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
