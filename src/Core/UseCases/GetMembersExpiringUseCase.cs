using ClubExample.Core.InputPorts;
using ClubExample.Core.InputPorts.Queries;
using ClubExample.Core.InputPorts.Results;
using ClubExample.Core.OutputPorts;

namespace ClubExample.Core.UseCases;

public sealed class GetMembersExpiringUseCase : IGetMembersExpiringUseCase
{
    private readonly IMemberRepository _memberRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetMembersExpiringUseCase(
        IMemberRepository memberRepository,
        ISubscriptionRepository subscriptionRepository)
    {
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
    }

    public async Task<IEnumerable<MemberExpiringResult>> ExecuteAsync(
        GetMembersExpiringQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.DaysUntilExpiration < 0)
        {
            throw new ArgumentException("Days until expiration cannot be negative.", nameof(query));
        }

        var members = await _memberRepository.GetMembersWithExpiringSubscriptionsAsync(
            query.DaysUntilExpiration, 
            cancellationToken);

        var results = new List<MemberExpiringResult>();
        
        foreach (var member in members)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(member.SubscriptionId, cancellationToken);
            
            if (subscription != null)
            {
                var daysUntilExpiration = (int)(subscription.EndDate.Date - DateTime.UtcNow.Date).TotalDays;
                
                results.Add(new MemberExpiringResult(
                    MemberId: member.Id,
                    Name: member.Name,
                    Email: member.Email,
                    SubscriptionId: subscription.Id,
                    SubscriptionEndDate: subscription.EndDate,
                    DaysUntilExpiration: daysUntilExpiration
                ));
            }
        }

        return results;
    }
}
