using ClubExample.Core.Domain;
using ClubExample.Core.InputPorts;
using ClubExample.Core.InputPorts.Commands;
using ClubExample.Core.InputPorts.Results;
using ClubExample.Core.OutputPorts;

namespace ClubExample.Core.UseCases;

public sealed class RegisterMemberUseCase : IRegisterMemberUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterMemberUseCase(
        IClubRepository clubRepository,
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository ?? throw new ArgumentNullException(nameof(clubRepository));
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<RegisterMemberResult> ExecuteAsync(
        RegisterMemberCommand command, 
        CancellationToken cancellationToken = default)
    {
        // Input validation
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        // Business rule: Club must exist
        var club = await _clubRepository.GetByIdAsync(command.ClubId, cancellationToken)
            ?? throw new InvalidOperationException($"Club with ID '{command.ClubId}' not found.");

        // Business rule: Member name must be unique within a club
        var memberExists = await _memberRepository.GetExistsByNameInClubAsync(
            command.Name, 
            command.ClubId, 
            cancellationToken);

        if (memberExists)
        {
            throw new InvalidOperationException(
                $"A member with name '{command.Name}' already exists in club '{club.Name}'.");
        }

        // Create domain entities
        var subscription = CreateSubscription(command.SubscriptionType);
        var member = CreateMember(command, subscription.Id);

        // Persist changes using Unit of Work pattern
        // All operations are tracked and committed in a single transaction
        await _unitOfWork.AddAsync(subscription, cancellationToken);
        await _unitOfWork.AddAsync(member, cancellationToken);
        
        // Commit all changes atomically - either everything saves or nothing does
        await _unitOfWork.CommitAsync(cancellationToken);

        // Return result
        return new RegisterMemberResult(
            MemberId: member.Id,
            SubscriptionId: subscription.Id,
            SubscriptionStartDate: subscription.StartDate,
            SubscriptionEndDate: subscription.EndDate
        );
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
}
