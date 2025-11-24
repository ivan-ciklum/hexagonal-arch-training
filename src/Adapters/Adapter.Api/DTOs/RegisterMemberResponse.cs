namespace ClubExample.Adapter.Api.DTOs;

public sealed record RegisterMemberResponse
{
    public required Guid MemberId { get; init; }
    public required Guid SubscriptionId { get; init; }
    public required DateTime SubscriptionStartDate { get; init; }
    public required DateTime SubscriptionEndDate { get; init; }
}
