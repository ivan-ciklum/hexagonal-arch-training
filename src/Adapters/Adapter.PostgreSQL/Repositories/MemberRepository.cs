using ClubExample.Core.Domain;
using ClubExample.Core.OutputPorts;
using Microsoft.EntityFrameworkCore;

namespace ClubExample.Adapter.PostgreSQL.Repositories;

public sealed class MemberRepository : IMemberRepository
{
    private readonly ClubDbContext _context;

    public MemberRepository(ClubDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<bool> GetExistsByNameInClubAsync(
        string name, 
        Guid clubId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .AnyAsync(m => m.Name == name && m.ClubId == clubId, cancellationToken);
    }
    
    public async Task<IEnumerable<Member>> GetMembersWithExpiringSubscriptionsAsync(
        int daysUntilExpiration, 
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(daysUntilExpiration);
        var today = DateTime.UtcNow.Date;
        
        return await (from member in _context.Members
                      join subscription in _context.Subscriptions 
                      on member.SubscriptionId equals subscription.Id
                      where subscription.EndDate >= today
                         && subscription.EndDate <= cutoffDate
                         && subscription.Status == "Active"
                      select member)
                     .ToListAsync(cancellationToken);
    }
}
