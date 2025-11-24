using ClubExample.Core.Domain;

namespace ClubExample.Core.OutputPorts;

public interface IMemberRepository
{
    /// <summary>
    /// Retrieves a member by their unique identifier.
    /// </summary>
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a member with the given name already exists in the specified club.
    /// </summary>
    Task<bool> GetExistsByNameInClubAsync(string name, Guid clubId, CancellationToken cancellationToken = default);
}
