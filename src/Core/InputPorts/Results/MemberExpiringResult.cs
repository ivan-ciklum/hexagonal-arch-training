namespace ClubExample.Core.InputPorts.Results;

public record MemberExpiringResult(
    Guid MemberId,
    string Name,
    string Email,
    Guid SubscriptionId,
    DateTime SubscriptionEndDate,
    int DaysUntilExpiration
);
