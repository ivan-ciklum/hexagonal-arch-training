namespace ClubExample.Adapter.Api.DTOs;

/// <summary>
/// Response after successfully registering a member
/// </summary>
public sealed record RegisterMemberResponse
{
    /// <summary>
    /// The unique identifier of the newly registered member
    /// </summary>
    /// <example>d290f1ee-6c54-4b01-90e6-d701748f0851</example>
    public required Guid MemberId { get; init; }
    
    /// <summary>
    /// The unique identifier of the member's subscription
    /// </summary>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public required Guid SubscriptionId { get; init; }
    
    /// <summary>
    /// The start date of the subscription
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public required DateTime SubscriptionStartDate { get; init; }
    
    /// <summary>
    /// The end date of the subscription
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public required DateTime SubscriptionEndDate { get; init; }
}
