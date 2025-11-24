using ClubExample.Core.Domain;

namespace ClubExample.Core.OutputPorts;

public interface ISubscriptionRepository
{
    /// <summary>
    /// Retrieves a subscription by its unique identifier.
    /// </summary>
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
