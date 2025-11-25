using ClubExample.Adapter.InMemoryData;
using ClubExample.Adapter.InMemoryData.Repositories;
using ClubExample.Core.Domain;
using ClubExample.Core.InputPorts.Commands;
using ClubExample.Core.OutputPorts;
using ClubExample.Core.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.UnitTest.UseCases;

/// <summary>
/// Unit tests for RegisterMemberUseCase.
/// These tests use in-memory repositories to test ONLY Core logic.
/// No database, no HTTP, no infrastructure - pure business logic testing.
/// Now uses Unit of Work pattern for transactional consistency and includes cache integration.
/// </summary>
[TestFixture]
public class RegisterMemberUseCaseTests
{
    private InMemoryUnitOfWork _unitOfWork = null!;
    private InMemoryClubRepository _clubRepository = null!;
    private InMemoryMemberRepository _memberRepository = null!;
    private InMemorySubscriptionRepository _subscriptionRepository = null!;
    private Mock<ICacheRepository> _cacheMock = null!;
    private Mock<IMessagePublisher> _messagePublisherMock = null!;
    private Mock<ILogger<RegisterMemberUseCase>> _loggerMock = null!;
    private RegisterMemberUseCase _useCase = null!;

    [SetUp]
    public void Setup()
    {
        // Arrange: Create fresh in-memory unit of work and repositories for each test
        _unitOfWork = new InMemoryUnitOfWork();
        _clubRepository = new InMemoryClubRepository(_unitOfWork);
        _memberRepository = new InMemoryMemberRepository(_unitOfWork);
        _subscriptionRepository = new InMemorySubscriptionRepository(_unitOfWork);
        
        // Mock the cache repository
        _cacheMock = new Mock<ICacheRepository>();
        
        // Mock the message publisher
        _messagePublisherMock = new Mock<IMessagePublisher>();
        
        // Mock the logger
        _loggerMock = new Mock<ILogger<RegisterMemberUseCase>>();
        
        _useCase = new RegisterMemberUseCase(
            _clubRepository,
            _memberRepository,
            _cacheMock.Object,
            _unitOfWork,
            _messagePublisherMock.Object,
            _loggerMock.Object
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

        // Verify cache operations were called
        _cacheMock.Verify(c => c.GetAsync<Club>(
            It.Is<string>(key => key.StartsWith("club:")), 
            It.IsAny<CancellationToken>()), 
            Times.Once, 
            "Cache should be checked for club");

        _cacheMock.Verify(c => c.RemoveAsync(
            It.Is<string>(key => key.Contains("members")), 
            It.IsAny<CancellationToken>()), 
            Times.Once, 
            "Club members cache should be invalidated after registration");

        // Verify domain event was published
        _messagePublisherMock.Verify(m => m.PublishAsync(
            It.Is<string>(topic => topic == "club.members.registered"),
            It.Is<object>(evt => evt != null),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "MemberRegisteredEvent should be published");
    }

    [Test]
    public async Task ExecuteAsync_WithCachedClub_ShouldUseCacheAndNotQueryDatabase()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        var club = new Club { Id = clubId, Name = "Cached Club" };

        // Setup cache to return the club
        var cacheKey = $"club:{clubId}";
        _cacheMock.Setup(c => c.GetAsync<Club>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(club);

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "Jane Doe",
            Email: "jane@example.com",
            SubscriptionType: "Monthly"
        );

        // Act
        var result = await _useCase.ExecuteAsync(command);

        // Assert
        Assert.That(result, Is.Not.Null);
        
        // Verify cache was queried
        _cacheMock.Verify(c => c.GetAsync<Club>(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify cache was NOT set (club was already cached)
        _cacheMock.Verify(c => c.SetAsync(cacheKey, It.IsAny<Club>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WithCacheMiss_ShouldQueryDatabaseAndCacheResult()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        var club = new Club { Id = clubId, Name = "Test Club" };
        await _clubRepository.AddAsync(club);

        // Setup cache to return null (cache miss)
        var cacheKey = $"club:{clubId}";
        _cacheMock.Setup(c => c.GetAsync<Club>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Club?)null);

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "Bob Smith",
            Email: "bob@example.com",
            SubscriptionType: "Annual"
        );

        // Act
        var result = await _useCase.ExecuteAsync(command);

        // Assert
        Assert.That(result, Is.Not.Null);

        // Verify cache was checked
        _cacheMock.Verify(c => c.GetAsync<Club>(cacheKey, It.IsAny<CancellationToken>()), Times.Once);

        // Verify cache was set with the club (30 minutes TTL)
        _cacheMock.Verify(c => c.SetAsync(
            cacheKey, 
            It.Is<Club>(cl => cl.Id == clubId && cl.Name == "Test Club"), 
            TimeSpan.FromMinutes(30), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WithCacheFailure_ShouldContinueWithDatabaseQuery()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        var club = new Club { Id = clubId, Name = "Test Club" };
        await _clubRepository.AddAsync(club);

        // Setup cache to throw exception (simulating cache failure)
        _cacheMock.Setup(c => c.GetAsync<Club>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "Alice Wonder",
            Email: "alice@example.com",
            SubscriptionType: "Monthly"
        );

        // Act
        var result = await _useCase.ExecuteAsync(command);

        // Assert - Operation should succeed despite cache failure
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MemberId, Is.Not.EqualTo(Guid.Empty));

        // Verify member was still created
        var members = _memberRepository.GetAll();
        Assert.That(members, Has.Count.EqualTo(1));
        Assert.That(members[0].Name, Is.EqualTo("Alice Wonder"));
    }

    [Test]
    public async Task ExecuteAsync_WithMessagingFailure_ShouldSucceedAndNotThrow()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        var club = new Club { Id = clubId, Name = "Test Club" };
        await _clubRepository.AddAsync(club);

        // Setup message publisher to throw exception (simulating messaging failure)
        _messagePublisherMock.Setup(m => m.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Message broker unavailable"));

        var command = new RegisterMemberCommand(
            ClubId: clubId,
            Name: "Charlie Brown",
            Email: "charlie@example.com",
            SubscriptionType: "Monthly"
        );

        // Act - Should not throw despite messaging failure
        var result = await _useCase.ExecuteAsync(command);

        // Assert - Operation should succeed despite messaging failure
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MemberId, Is.Not.EqualTo(Guid.Empty));

        // Verify member was still created
        var members = _memberRepository.GetAll();
        Assert.That(members, Has.Count.EqualTo(1));
        Assert.That(members[0].Name, Is.EqualTo("Charlie Brown"));
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
