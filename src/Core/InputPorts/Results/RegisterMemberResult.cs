namespace ClubExample.Core.InputPorts.Results;

public sealed record RegisterMemberResult(
    Guid MemberId,
    Guid SubscriptionId,
    DateTime SubscriptionStartDate,
    DateTime SubscriptionEndDate
);
