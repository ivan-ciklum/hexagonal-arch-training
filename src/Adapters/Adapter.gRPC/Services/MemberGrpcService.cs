using ClubExample.Core.InputPorts;
using ClubExample.Core.InputPorts.Commands;
using Grpc.Core;

namespace ClubExample.Adapter.gRPC.Services;

public class MemberGrpcService : MemberService.MemberServiceBase
{
    private readonly IRegisterMemberUseCase _registerMemberUseCase;
    private readonly ILogger<MemberGrpcService> _logger;

    public MemberGrpcService(
        IRegisterMemberUseCase registerMemberUseCase,
        ILogger<MemberGrpcService> logger)
    {
        _registerMemberUseCase = registerMemberUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RegisterMember gRPC call.
    /// Maps gRPC message to Core command, executes use case, and maps result back.
    /// </summary>
    public override async Task<RegisterMemberResponse> RegisterMember(
        RegisterMemberRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation(
                "Received gRPC RegisterMember request for club {ClubId}, member {Name}",
                request.ClubId,
                request.Name);

            // Validate and parse GUID
            if (!Guid.TryParse(request.ClubId, out var clubId))
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Invalid ClubId format: {request.ClubId}"));
            }

            // Map gRPC request to Core command
            var command = new RegisterMemberCommand(
                ClubId: clubId,
                Name: request.Name,
                Email: request.Email,
                SubscriptionType: request.SubscriptionType
            );

            // Execute use case (Core business logic)
            var result = await _registerMemberUseCase.ExecuteAsync(
                command,
                context.CancellationToken);

            // Map Core result to gRPC response
            var response = new RegisterMemberResponse
            {
                MemberId = result.MemberId.ToString(),
                SubscriptionId = result.SubscriptionId.ToString(),
                SubscriptionStartDate = result.SubscriptionStartDate.ToString("O"), // ISO 8601
                SubscriptionEndDate = result.SubscriptionEndDate.ToString("O")
            };

            _logger.LogInformation(
                "Successfully registered member {MemberId} via gRPC",
                result.MemberId);

            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in gRPC RegisterMember");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation in gRPC RegisterMember");
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in gRPC RegisterMember");
            throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred"));
        }
    }
}
