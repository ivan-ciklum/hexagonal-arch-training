using NetArchTest.Rules;

namespace Architecture.UnitTest;

/// <summary>
/// Architecture tests to ensure Redis adapter follows hexagonal architecture principles.
/// </summary>
[TestFixture]
public class RedisAdapterArchitectureTests
{
    [Test]
    public void RedisAdapter_Should_Only_Depend_On_Core()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterRedisAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                SolutionTypes.AdapterApiNamespace,
                SolutionTypes.AdapterPostgreSQLNamespace,
                "ClubExample.Adapter.InMemoryData",
                "ClubExample.Adapter.gRPC",
                "ClubExample.Adapter.Pulsar"
            )
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Adapter.Redis should only depend on Core and Microsoft.Extensions libraries, not on other adapters. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void RedisAdapter_Should_Be_In_Repositories_Namespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterRedisAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespace($"{SolutionTypes.AdapterRedisNamespace}.Repositories")
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"All repository implementations in Adapter.Redis should be in the Repositories namespace. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void RedisAdapter_Should_Implement_CacheRepository_Interface()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterRedisAssembly)
            .That()
            .HaveNameEndingWith("CacheRepository")
            .Should()
            .ImplementInterface(typeof(ClubExample.Core.OutputPorts.ICacheRepository))
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Cache repositories in Adapter.Redis should implement ICacheRepository from Core. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void RedisAdapter_Repositories_Should_Be_Sealed()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterRedisAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Repository implementations should be sealed to prevent inheritance. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void RedisAdapter_Should_Not_Reference_EntityFramework()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterRedisAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql.EntityFrameworkCore.PostgreSQL"
            )
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"Adapter.Redis should not depend on Entity Framework (it's a cache, not a database adapter). " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void RedisAdapter_DependencyInjection_Should_Be_Static()
    {
        // Arrange & Act
        var result = Types.InAssembly(SolutionTypes.AdapterRedisAssembly)
            .That()
            .HaveNameEndingWith("DependencyInjection")
            .Should()
            .BeSealed()
            .And()
            .BeStatic()
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"DependencyInjection class should be static with extension methods. " +
            $"Violating types: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }
}
