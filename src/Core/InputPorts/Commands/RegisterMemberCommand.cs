namespace ClubExample.Core.InputPorts.Commands;

public sealed record RegisterMemberCommand(
    Guid ClubId,
    string Name,
    string Email,
    string SubscriptionType
);
