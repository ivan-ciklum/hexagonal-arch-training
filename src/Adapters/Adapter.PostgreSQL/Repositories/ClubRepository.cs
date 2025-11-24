using ClubExample.Core.Domain;
using ClubExample.Core.OutputPorts;
using Microsoft.EntityFrameworkCore;

namespace ClubExample.Adapter.PostgreSQL.Repositories;

public sealed class ClubRepository : IClubRepository
{
    private readonly ClubDbContext _context;

    public ClubRepository(ClubDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Club?> GetByIdAsync(Guid clubId, CancellationToken cancellationToken = default)
    {
        return await _context.Clubs
            .FirstOrDefaultAsync(c => c.Id == clubId, cancellationToken);
    }
}
