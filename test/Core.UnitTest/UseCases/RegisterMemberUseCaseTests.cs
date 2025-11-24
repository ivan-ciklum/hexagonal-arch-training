using ClubExample.Adapter.InMemoryData;
using ClubExample.Adapter.InMemoryData.Repositories;
using ClubExample.Core.Domain;
using ClubExample.Core.InputPorts.Commands;
using ClubExample.Core.UseCases;

namespace Core.UnitTest.UseCases;

/// <summary>
/// Unit tests for RegisterMemberUseCase.
/// These tests use in-memory repositories to test ONLY Core logic.
/// No database, no HTTP, no infrastructure - pure business logic testing.
/// Now uses Unit of Work pattern for transactional consistency.
/// </summary>
[TestFixture]
public class RegisterMemberUseCaseTests
{
    private InMemoryUnitOfWork _unitOfWork = null!;
    private InMemoryClubRepository _clubRepository = null!;
    private InMemoryMemberRepository _memberRepository = null!;
    private InMemorySubscriptionRepository _subscriptionRepository = null!;
    private RegisterMemberUseCase _useCase = null!;

    [SetUp]
    public void Setup()
    {
        // Arrange: Create fresh in-memory unit of work and repositories for each test
        _unitOfWork = new InMemoryUnitOfWork();
        _clubRepository = new InMemoryClubRepository(_unitOfWork);
        _memberRepository = new InMemoryMemberRepository(_unitOfWork);
        _subscriptionRepository = new InMemorySubscriptionRepository(_unitOfWork);
        
        _useCase = new RegisterMemberUseCase(
            _clubRepository,
            _memberRepository,
            _unitOfWork
        );
    }

    [Test]
    public async Task ExecuteAsync_WithValidCommand_ShouldCreateMemberAndSubscription()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        var club = new Club { Id = clubId, Name = "Test Club" };
        await _clubRepository.AddAsync(club);

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "John Doe",
            Email: "john@example.com",
            SubscriptionType: "Monthly"
        );

        // Act
        var result = await _useCase.ExecuteAsync(command);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MemberId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.SubscriptionId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.SubscriptionStartDate, Is.LessThanOrEqualTo(DateTime.UtcNow));
        Assert.That(result.SubscriptionEndDate, Is.GreaterThan(result.SubscriptionStartDate));

        // Verify member was created
        var members = _memberRepository.GetAll();
        Assert.That(members, Has.Count.EqualTo(1));
        Assert.That(members[0].Name, Is.EqualTo("John Doe"));
        Assert.That(members[0].Email, Is.EqualTo("john@example.com"));
        Assert.That(members[0].ClubId, Is.EqualTo(clubId));

        // Verify subscription was created
        var subscriptions = _subscriptionRepository.GetAll();
        Assert.That(subscriptions, Has.Count.EqualTo(1));
        Assert.That(subscriptions[0].Type, Is.EqualTo("Monthly"));
        Assert.That(subscriptions[0].Status, Is.EqualTo("Active"));
    }

    [Test]
    public async Task ExecuteAsync_WithMonthlySubscription_ShouldSetCorrectEndDate()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        await _clubRepository.AddAsync(new Club { Id = clubId, Name = "Test Club" });

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "Jane Doe",
            Email: "jane@example.com",
            SubscriptionType: "Monthly"
        );

        // Act
        var result = await _useCase.ExecuteAsync(command);

        // Assert
        var expectedEndDate = DateTime.UtcNow.AddMonths(1);
        Assert.That(result.SubscriptionEndDate.Date, Is.EqualTo(expectedEndDate.Date));
    }

    [Test]
    public async Task ExecuteAsync_WithAnnualSubscription_ShouldSetCorrectEndDate()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        await _clubRepository.AddAsync(new Club { Id = clubId, Name = "Test Club" });

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "Bob Smith",
            Email: "bob@example.com",
            SubscriptionType: "Annual"
        );

        // Act
        var result = await _useCase.ExecuteAsync(command);

        // Assert
        var expectedEndDate = DateTime.UtcNow.AddYears(1);
        Assert.That(result.SubscriptionEndDate.Date, Is.EqualTo(expectedEndDate.Date));
    }

    [Test]
    public void ExecuteAsync_WithNonExistentClub_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new RegisterMemberCommand(
            ClubId: Guid.NewGuid(),
            Name: "John Doe",
            Email: "john@example.com",
            SubscriptionType: "Monthly"
        );

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _useCase.ExecuteAsync(command)
        );

        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task ExecuteAsync_WithDuplicateMemberName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        await _clubRepository.AddAsync(new Club { Id = clubId, Name = "Test Club" });

        // Register first member
        var firstCommand = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "John Doe",
            Email: "john1@example.com",
            SubscriptionType: "Monthly"
        );
        await _useCase.ExecuteAsync(firstCommand);

        // Try to register another member with the same name
        var duplicateCommand = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "John Doe",
            Email: "john2@example.com",
            SubscriptionType: "Monthly"
        );

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _useCase.ExecuteAsync(duplicateCommand)
        );

        Assert.That(ex!.Message, Does.Contain("already exists"));
    }

    [Test]
    public void ExecuteAsync_WithEmptyClubId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new RegisterMemberCommand(
            ClubId: Guid.Empty,
            Name: "John Doe",
            Email: "john@example.com",
            SubscriptionType: "Monthly"
        );

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _useCase.ExecuteAsync(command)
        );

        Assert.That(ex!.Message, Does.Contain("ClubId"));
    }

    [Test]
    public void ExecuteAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new RegisterMemberCommand(
            ClubId: Guid.NewGuid(),
            Name: "",
            Email: "john@example.com",
            SubscriptionType: "Monthly"
        );

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _useCase.ExecuteAsync(command)
        );

        Assert.That(ex!.Message, Does.Contain("name"));
    }

    [Test]
    public void ExecuteAsync_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new RegisterMemberCommand(
            ClubId: Guid.NewGuid(),
            Name: "John Doe",
            Email: "not-an-email",
            SubscriptionType: "Monthly"
        );

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _useCase.ExecuteAsync(command)
        );

        Assert.That(ex!.Message, Does.Contain("email"));
    }

    [Test]
    public void ExecuteAsync_WithInvalidSubscriptionType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        _clubRepository.AddAsync(new Club { Id = clubId, Name = "Test Club" }).Wait();

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "John Doe",
            Email: "john@example.com",
            SubscriptionType: "InvalidType"
        );

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _useCase.ExecuteAsync(command)
        );

        Assert.That(ex!.Message, Does.Contain("Unknown subscription type"));
    }

    [Test]
    public void ExecuteAsync_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _useCase.ExecuteAsync(null!)
        );
    }
}
