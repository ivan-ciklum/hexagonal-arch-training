namespace ClubExample.Adapter.Api.DTOs;

public sealed record RegisterMemberRequest
{
    public required Guid ClubId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string SubscriptionType { get; init; }
}
