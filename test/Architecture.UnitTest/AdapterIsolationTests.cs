using NetArchTest.Rules;

namespace Architecture.UnitTest;

/// <summary>
/// Architecture tests to ensure adapters remain isolated from each other.
/// Adapters should only depend on Core, never on other adapters.
/// </summary>
[TestFixture]
public class AdapterIsolationTests
{
    [Test]
    public void AdapterApi_Should_Not_Reference_AdapterPostgreSQL()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterApiAssembly)
            .Should()
            .NotHaveDependencyOn(SolutionTypes.AdapterPostgreSQLNamespace)
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Adapter.Api should not depend on Adapter.PostgreSQL. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void AdapterPostgreSQL_Should_Not_Reference_AdapterApi()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterPostgreSQLAssembly)
            .Should()
            .NotHaveDependencyOn(SolutionTypes.AdapterApiNamespace)
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Adapter.PostgreSQL should not depend on Adapter.Api. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void AdapterApi_Should_Only_Depend_On_Core()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterApiAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                SolutionTypes.AdapterPostgreSQLNamespace,
                "ClubExample.Adapter.InMemoryData",
                "ClubExample.Adapter.Redis",
                "ClubExample.Adapter.Pulsar"
            )
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Adapter.Api should only depend on Core, not on other adapters. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void AdapterPostgreSQL_Should_Only_Depend_On_Core()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterPostgreSQLAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                SolutionTypes.AdapterApiNamespace,
                "ClubExample.Adapter.InMemoryData",
                "ClubExample.Adapter.Redis",
                "ClubExample.Adapter.Pulsar"
            )
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Adapter.PostgreSQL should only depend on Core, not on other adapters. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }
}
