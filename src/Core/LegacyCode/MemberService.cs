using ClubExample.Core.Domain;

namespace ClubExample.Core.LegacyCode;

public class MemberService
{
    
    
    public async Task StartMemberSubscription(Guid clubId, string memberName, string email, string subscriptionType)
    {
        var clubDal = new ClubDal();
        var club = await clubDal.GetByIdAsync(clubId);
        if (club == null)
            throw new InvalidOperationException("Club not found.");

        var memberDal = new MemberDal();
        Member? memberFound = await memberDal.GetByName(memberName, clubId);
        if (memberFound is not null)
        {
            throw new InvalidOperationException("Member name exist in club.");
        }
            
        // Create Subscription
        var subscription = new Subscription
                           {
                               Id = Guid.NewGuid(),
                               Type = subscriptionType,
                               StartDate = DateTime.UtcNow,
                               EndDate = subscriptionType switch
                                         {
                                             "Monthly" => DateTime.UtcNow.AddMonths(1),
                                             "Annual" => DateTime.UtcNow.AddYears(1),
                                             _ => throw new InvalidOperationException("Unknown subscription type.")
                                         },
                               Status = nameof(SubscriptionStatus.Active)
                           };

        // Create member
        var member = new Member
                     {
                         Id = Guid.NewGuid(),
                         ClubId = clubId,
                         Name = memberName,
                         Email = email,
                         SubscriptionId = subscription.Id
                     };
        SubscriptionDal subscriptionDal = new SubscriptionDal();
        
        // using transaccion
        using var transaction = new TransactionFactory().Create();
        transaction.Begin();
        
        try
        {
            await memberDal.InsertAsync(member);
            await subscriptionDal.InsertAsync(subscription);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}