using ClubExample.Core.Domain;
using ClubExample.Core.Domain.Events;
using ClubExample.Core.InputPorts;
using ClubExample.Core.InputPorts.Commands;
using ClubExample.Core.InputPorts.Results;
using ClubExample.Core.OutputPorts;
using Microsoft.Extensions.Logging;

namespace ClubExample.Core.UseCases;

/// <summary>
/// Use case for registering a new member in a club.
/// Demonstrates cache integration and event publishing following hexagonal architecture principles.
/// </summary>
public sealed class RegisterMemberUseCase : IRegisterMemberUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICacheRepository _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<RegisterMemberUseCase> _logger;

    // Cache keys follow a consistent naming convention
    private const string ClubCacheKeyPrefix = "club:";
    private const string ClubMembersCacheKeyPrefix = "club:members:";
    
    // Topic names for event publishing
    private const string MemberEventsTopicName = "club.members.registered";

    public RegisterMemberUseCase(
        IClubRepository clubRepository,
        IMemberRepository memberRepository,
        ICacheRepository cache,
        IUnitOfWork unitOfWork,
        IMessagePublisher messagePublisher,
        ILogger<RegisterMemberUseCase> logger)
    {
        _clubRepository = clubRepository ?? throw new ArgumentNullException(nameof(clubRepository));
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RegisterMemberResult> ExecuteAsync(
        RegisterMemberCommand command, 
        CancellationToken cancellationToken = default)
    {
        // Input validation
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        var club = await GetClubWithCacheAsync(command.ClubId, cancellationToken)
            ?? throw new InvalidOperationException($"Club with ID '{command.ClubId}' not found.");

        var memberExists = await _memberRepository.GetExistsByNameInClubAsync(
            command.Name, 
            command.ClubId, 
            cancellationToken);

        if (memberExists)
        {
            throw new InvalidOperationException(
                $"A member with name '{command.Name}' already exists in club '{club.Name}'.");
        }

        var subscription = CreateSubscription(command.SubscriptionType);
        var member = CreateMember(command, subscription.Id);

        await _unitOfWork.AddAsync(subscription, cancellationToken);
        await _unitOfWork.AddAsync(member, cancellationToken);        
        await _unitOfWork.CommitAsync(cancellationToken);

        await InvalidateClubRelatedCachesAsync(command.ClubId, cancellationToken);

        await PublishMemberRegisteredEventAsync(member, subscription, command.SubscriptionType, cancellationToken);

        return new RegisterMemberResult(
            MemberId: member.Id,
            SubscriptionId: subscription.Id,
            SubscriptionStartDate: subscription.StartDate,
            SubscriptionEndDate: subscription.EndDate
        );
    }

    /// <summary>
    /// Gets a club using cache-aside pattern.
    /// </summary>
    private async Task<Club?> GetClubWithCacheAsync(Guid clubId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{ClubCacheKeyPrefix}{clubId}";

        try
        {
            var cachedClub = await _cache.GetAsync<Club>(cacheKey, cancellationToken);
            if (cachedClub is not null)
            {
                return cachedClub;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Cache read failure for club {ClubId} with key {CacheKey}. Falling back to database query. Error: {ErrorMessage}",
                clubId, cacheKey, ex.Message);
        }

        var club = await _clubRepository.GetByIdAsync(clubId, cancellationToken);

        if (club is not null)
        {
            try
            {
                await _cache.SetAsync(cacheKey, club, TimeSpan.FromMinutes(30), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Cache write failure for club {ClubId} with key {CacheKey}. Operation will continue without caching. Error: {ErrorMessage}",
                    clubId, cacheKey, ex.Message);
            }
        }

        return club;
    }

    /// <summary>
    /// Invalidates caches related to a club after a member is registered.
    /// This ensures cache consistency with the database.
    /// </summary>
    private async Task InvalidateClubRelatedCachesAsync(Guid clubId, CancellationToken cancellationToken)
    {
        var clubMembersCacheKey = $"{ClubMembersCacheKeyPrefix}{clubId}";
        
        try
        {
            await _cache.RemoveAsync(clubMembersCacheKey, cancellationToken);
            _logger.LogInformation(
                "Successfully invalidated cache for club members with key {CacheKey}",
                clubMembersCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Cache invalidation failure for club {ClubId} with key {CacheKey}. " +
                "The cache will expire naturally via TTL, preventing stale data indefinitely. Error: {ErrorMessage}",
                clubId, clubMembersCacheKey, ex.Message);
        }
    }

    private static void ValidateCommand(RegisterMemberCommand command)
    {
        if (command.ClubId == Guid.Empty)
            throw new ArgumentException("ClubId cannot be empty.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Member name is required.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Member email is required.", nameof(command));

        if (!IsValidEmail(command.Email))
            throw new ArgumentException("Invalid email format.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.SubscriptionType))
            throw new ArgumentException("Subscription type is required.", nameof(command));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static Subscription CreateSubscription(string subscriptionType)
    {
        var startDate = DateTime.UtcNow;
        var endDate = subscriptionType.ToUpperInvariant() switch
        {
            "MONTHLY" => startDate.AddMonths(1),
            "ANNUAL" => startDate.AddYears(1),
            _ => throw new InvalidOperationException(
                $"Unknown subscription type: '{subscriptionType}'. Valid types are: Monthly, Annual.")
        };

        return new Subscription
        {
            Id = Guid.NewGuid(),
            Type = subscriptionType,
            StartDate = startDate,
            EndDate = endDate,
            Status = nameof(SubscriptionStatus.Active)
        };
    }

    private static Member CreateMember(RegisterMemberCommand command, Guid subscriptionId)
    {
        return new Member
        {
            Id = Guid.NewGuid(),
            ClubId = command.ClubId,
            Name = command.Name,
            Email = command.Email,
            SubscriptionId = subscriptionId
        };
    }

    /// <summary>
    /// Publishes a MemberRegisteredEvent to notify other systems/services.
    /// Follows the "outbox pattern" principle - publish after successful commit.
    /// In production, consider using transactional outbox pattern for guaranteed delivery.
    /// </summary>
    private async Task PublishMemberRegisteredEventAsync(
        Member member,
        Subscription subscription,
        string subscriptionType,
        CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = new MemberRegisteredEvent
            {
                MemberId = member.Id,
                ClubId = member.ClubId,
                MemberName = member.Name,
                MemberEmail = member.Email,
                SubscriptionId = subscription.Id,
                SubscriptionType = subscriptionType,
                SubscriptionStartDate = subscription.StartDate,
                SubscriptionEndDate = subscription.EndDate
            };

            await _messagePublisher.PublishAsync(
                MemberEventsTopicName,
                domainEvent,
                cancellationToken);
            
            _logger.LogInformation(
                "Successfully published MemberRegisteredEvent for member {MemberId} in club {ClubId} to topic {TopicName}",
                member.Id, member.ClubId, MemberEventsTopicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Event publishing failure for member {MemberId} in club {ClubId} to topic {TopicName}. " +
                "The member registration has been successfully saved to the database, but the event notification failed. " +
                "This requires manual intervention or retry logic. Error: {ErrorMessage}",
                member.Id, member.ClubId, MemberEventsTopicName, ex.Message);
        }
    }
}
