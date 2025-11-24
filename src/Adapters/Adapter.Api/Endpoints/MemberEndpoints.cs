using ClubExample.Adapter.Api.DTOs;
using ClubExample.Core.InputPorts;
using ClubExample.Core.InputPorts.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ClubExample.Adapter.Api.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members")
            .WithOpenApi();

        group.MapPost("/", RegisterMember)
            .WithName("RegisterMember")
            .WithSummary("Register a new member in a club")
            .Produces<RegisterMemberResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> RegisterMember(
        RegisterMemberRequest request,
        IRegisterMemberUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            // Map DTO to Core command
            var command = new RegisterMemberCommand(
                ClubId: request.ClubId,
                Name: request.Name,
                Email: request.Email,
                SubscriptionType: request.SubscriptionType
            );

            // Execute use case
            var result = await useCase.ExecuteAsync(command, cancellationToken);

            // Map Core result to DTO
            var response = new RegisterMemberResponse
            {
                MemberId = result.MemberId,
                SubscriptionId = result.SubscriptionId,
                SubscriptionStartDate = result.SubscriptionStartDate,
                SubscriptionEndDate = result.SubscriptionEndDate
            };

            return Results.Created($"/api/members/{response.MemberId}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error"
            );
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Business Rule Violation"
            );
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error"
            );
        }
    }
}
