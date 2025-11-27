namespace ClubExample.Adapter.Api.DTOs;

public sealed class MemberExpiringResponse
{
    public required Guid MemberId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required Guid SubscriptionId { get; init; }
    public required DateTime SubscriptionEndDate { get; init; }
    public required int DaysUntilExpiration { get; init; }
}
