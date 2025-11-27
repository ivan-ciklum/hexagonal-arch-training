namespace ClubExample.Core.InputPorts.Queries;

public record GetMembersExpiringQuery(
    int DaysUntilExpiration
);
