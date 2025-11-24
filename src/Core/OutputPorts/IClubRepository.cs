using ClubExample.Core.Domain;

namespace ClubExample.Core.OutputPorts;

public interface IClubRepository
{
    /// <summary>
    /// Retrieves a club by its unique identifier.
    /// </summary>
    /// <returns>The club if found, null otherwise</returns>
    Task<Club?> GetByIdAsync(Guid clubId, CancellationToken cancellationToken = default);
}
