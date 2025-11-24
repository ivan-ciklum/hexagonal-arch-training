using ClubExample.Core.Domain;
using ClubExample.Core.OutputPorts;

namespace ClubExample.Adapter.InMemoryData.Repositories;

/// <summary>
/// In-memory implementation of IClubRepository for unit testing.
/// </summary>
public sealed class InMemoryClubRepository : IClubRepository
{
    private readonly InMemoryUnitOfWork _unitOfWork;

    public InMemoryClubRepository(InMemoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public Task<Club?> GetByIdAsync(Guid clubId, CancellationToken cancellationToken = default)
    {
        var club = _unitOfWork.GetAll<Club>().FirstOrDefault(c => c.Id == clubId);
        return Task.FromResult(club);
    }

    // Helper methods for testing
    public async Task AddAsync(Club club)
    {
        await _unitOfWork.AddAsync(club);
        await _unitOfWork.CommitAsync();
    }

    public IReadOnlyList<Club> GetAll() => _unitOfWork.GetAll<Club>();
}
