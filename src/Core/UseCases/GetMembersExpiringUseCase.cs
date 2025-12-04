using ClubExample.Core.InputPorts;
using ClubExample.Core.InputPorts.Queries;
using ClubExample.Core.InputPorts.Results;
using ClubExample.Core.OutputPorts;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace ClubExample.Core.UseCases;

public sealed class GetMembersExpiringUseCase : IGetMembersExpiringUseCase
{
    private static readonly ActivitySource ActivitySource = new("ClubExample.Core");
    
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
        using var activity = ActivitySource.StartActivity("GetMembersExpiring", ActivityKind.Internal);
        activity?.SetTag("usecase.name", "GetMembersExpiring");
        activity?.SetTag("query.days_until_expiration", query.DaysUntilExpiration);

        try
        {
            if (query.DaysUntilExpiration < 0)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid days until expiration");
                throw new ArgumentException("Days until expiration cannot be negative.", nameof(query));
            }

            var members = await _memberRepository.GetMembersWithExpiringSubscriptionsAsync(
                query.DaysUntilExpiration, 
                cancellationToken);

            var membersList = members.ToList();
            activity?.SetTag("query.members_count", membersList.Count);

            var results = new List<MemberExpiringResult>();
            
            foreach (var member in membersList)
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

            activity?.SetTag("query.results_count", results.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return results;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
