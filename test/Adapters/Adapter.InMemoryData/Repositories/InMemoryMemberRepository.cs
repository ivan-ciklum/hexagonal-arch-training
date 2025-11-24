using ClubExample.Core.Domain;
using ClubExample.Core.OutputPorts;

namespace ClubExample.Adapter.InMemoryData.Repositories;

/// <summary>
/// In-memory implementation of IMemberRepository for unit testing.
/// This allows testing Core logic without any real database.
/// Now focused only on queries - persistence is handled by Unit of Work.
/// </summary>
public sealed class InMemoryMemberRepository : IMemberRepository
{
    private readonly InMemoryUnitOfWork _unitOfWork;

    public InMemoryMemberRepository(InMemoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = _unitOfWork.GetAll<Member>().FirstOrDefault(m => m.Id == id);
        return Task.FromResult(member);
    }

    public Task<bool> GetExistsByNameInClubAsync(string name, Guid clubId, CancellationToken cancellationToken = default)
    {
        var exists = _unitOfWork.GetAll<Member>().Any(m => m.Name == name && m.ClubId == clubId);
        return Task.FromResult(exists);
    }

    // Helper methods for testing
    public IReadOnlyList<Member> GetAll() => _unitOfWork.GetAll<Member>();
}
