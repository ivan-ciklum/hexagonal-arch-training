using ClubExample.Core.Domain;
using ClubExample.Core.OutputPorts;

namespace ClubExample.Adapter.InMemoryData.Repositories;

/// <summary>
/// In-memory implementation of ISubscriptionRepository for unit testing.
/// Now focused only on queries - persistence is handled by Unit of Work.
/// </summary>
public sealed class InMemorySubscriptionRepository : ISubscriptionRepository
{
    private readonly InMemoryUnitOfWork _unitOfWork;

    public InMemorySubscriptionRepository(InMemoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = _unitOfWork.GetAll<Subscription>().FirstOrDefault(s => s.Id == id);
        return Task.FromResult(subscription);
    }

    // Helper methods for testing
    public IReadOnlyList<Subscription> GetAll() => _unitOfWork.GetAll<Subscription>();
}
