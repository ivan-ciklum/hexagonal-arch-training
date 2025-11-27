using ClubExample.Core.InputPorts.Queries;
using ClubExample.Core.InputPorts.Results;

namespace ClubExample.Core.InputPorts;

public interface IGetMembersExpiringUseCase
{
    Task<IEnumerable<MemberExpiringResult>> ExecuteAsync(
        GetMembersExpiringQuery query, 
        CancellationToken cancellationToken = default);
}
