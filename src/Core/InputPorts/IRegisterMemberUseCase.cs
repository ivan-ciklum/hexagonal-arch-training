using ClubExample.Core.InputPorts.Commands;
using ClubExample.Core.InputPorts.Results;

namespace ClubExample.Core.InputPorts;

public interface IRegisterMemberUseCase
{
    /// <summary>
    /// Registers a new member in the specified club with a subscription.
    /// </summary>
    /// <param name="command">The command containing member registration details</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Result containing the created member and subscription information</returns>
    /// <exception cref="InvalidOperationException">When business rules are violated</exception>
    Task<RegisterMemberResult> ExecuteAsync(RegisterMemberCommand command, CancellationToken cancellationToken = default);
}
