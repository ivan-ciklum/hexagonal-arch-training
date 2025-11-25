namespace ClubExample.Core.Domain.Events;

/// <summary>
/// Domain event raised when a new member is successfully registered in a club.
/// This event can be consumed by other services/contexts for:
/// - Sending welcome emails
/// - Updating analytics
/// - Triggering onboarding workflows
/// - Synchronizing with external systems
/// </summary>
public sealed record MemberRegisteredEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred (UTC)
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The ID of the registered member
    /// </summary>
    public required Guid MemberId { get; init; }

    /// <summary>
    /// The ID of the club the member joined
    /// </summary>
    public required Guid ClubId { get; init; }

    /// <summary>
    /// Member's name
    /// </summary>
    public required string MemberName { get; init; }

    /// <summary>
    /// Member's email
    /// </summary>
    public required string MemberEmail { get; init; }

    /// <summary>
    /// The ID of the subscription created for this member
    /// </summary>
    public required Guid SubscriptionId { get; init; }

    /// <summary>
    /// Type of subscription (Monthly, Annual, etc.)
    /// </summary>
    public required string SubscriptionType { get; init; }

    /// <summary>
    /// Subscription start date
    /// </summary>
    public required DateTime SubscriptionStartDate { get; init; }

    /// <summary>
    /// Subscription end date
    /// </summary>
    public required DateTime SubscriptionEndDate { get; init; }

    /// <summary>
    /// Optional: Event version for schema evolution
    /// </summary>
    public int Version { get; init; } = 1;
}
