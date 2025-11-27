using ClubExample.Adapter.Api.DTOs;
using ClubExample.Core.InputPorts;
using ClubExample.Core.InputPorts.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ClubExample.Adapter.Api.Endpoints;

public static class MemberQueryEndpoints
{
    public static void MapMemberQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members - Queries")
            .WithOpenApi();

        group.MapGet("/expiring", GetMembersExpiring)
            .WithName("GetMembersExpiring")
            .WithSummary("Get members with subscriptions expiring within specified days")
            .Produces<IEnumerable<MemberExpiringResponse>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetMembersExpiring(
        [FromQuery] int days,
        IGetMembersExpiringUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            if (days < 0)
            {
                return Results.Problem(
                    detail: "Days parameter must be a positive number.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid Parameter"
                );
            }

            // Create query
            var query = new GetMembersExpiringQuery(DaysUntilExpiration: days);

            // Execute use case
            var results = await useCase.ExecuteAsync(query, cancellationToken);

            // Map Core results to DTOs
            var response = results.Select(r => new MemberExpiringResponse
            {
                MemberId = r.MemberId,
                Name = r.Name,
                Email = r.Email,
                SubscriptionId = r.SubscriptionId,
                SubscriptionEndDate = r.SubscriptionEndDate,
                DaysUntilExpiration = r.DaysUntilExpiration
            });

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error"
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
