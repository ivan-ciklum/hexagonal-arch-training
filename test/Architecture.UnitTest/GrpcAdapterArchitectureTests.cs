using NetArchTest.Rules;
using NUnit.Framework;

namespace Architecture.UnitTest;

/// <summary>
/// Architectural tests to validate gRPC adapter follows hexagonal architecture principles.
/// </summary>
[TestFixture]
public class GrpcAdapterArchitectureTests
{
    private const string AdapterGrpcNamespace = "ClubExample.Adapter.gRPC";
    private const string CoreNamespace = "ClubExample.Core";

    [Test]
    public void GrpcAdapter_Should_OnlyDependOnCore()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(ClubExample.Adapter.gRPC.Services.MemberGrpcService).Assembly)
            .That()
            .ResideInNamespace(AdapterGrpcNamespace)
            .Should()
            .NotHaveDependencyOnAll(
                "ClubExample.Adapter.Api",
                "ClubExample.Adapter.PostgreSQL",
                "ClubExample.Host.Api"
            )
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"gRPC Adapter should not depend on other adapters or Host. Violations: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Test]
    public void GrpcAdapter_Should_NotBeReferencedByCore()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(ClubExample.Core.InputPorts.IRegisterMemberUseCase).Assembly)
            .Should()
            .NotHaveDependencyOn(AdapterGrpcNamespace)
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            "Core should never depend on gRPC Adapter (dependency inversion principle)");
    }

    [Test]
    public void GrpcServices_Should_EndWithGrpcService()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(ClubExample.Adapter.gRPC.Services.MemberGrpcService).Assembly)
            .That()
            .ResideInNamespace($"{AdapterGrpcNamespace}.Services")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("GrpcService")
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"All gRPC service classes should end with 'GrpcService'. Violations: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Test]
    public void GrpcAdapter_Should_NotContainBusinessLogic()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(ClubExample.Adapter.gRPC.Services.MemberGrpcService).Assembly)
            .That()
            .ResideInNamespace(AdapterGrpcNamespace)
            .Should()
            .NotHaveDependencyOn("System.Data")
            .And()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            "gRPC Adapter should not contain data access logic - it's a presentation layer");
    }

    [Test]
    public void GrpcServices_Should_HavePublicConstructorWithDependencyInjection()
    {
        // Arrange
        var grpcServiceTypes = Types.InAssembly(typeof(ClubExample.Adapter.gRPC.Services.MemberGrpcService).Assembly)
            .That()
            .ResideInNamespace($"{AdapterGrpcNamespace}.Services")
            .And()
            .AreClasses()
            .GetTypes();

        // Act & Assert
        foreach (var serviceType in grpcServiceTypes)
        {
            var publicConstructors = serviceType.GetConstructors(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            Assert.That(publicConstructors, Is.Not.Empty);
            
            // Verify at least one constructor has parameters (DI)
            Assert.That(publicConstructors.Any(c => c.GetParameters().Length > 0), Is.True,
                $"{serviceType.Name} should have constructor with dependency injection parameters");
        }
    }

    [Test]
    public void ProtoFiles_Should_ExistInProtosFolder()
    {
        // Arrange
        var assembly = typeof(ClubExample.Adapter.gRPC.Services.MemberGrpcService).Assembly;
        var assemblyLocation = System.IO.Path.GetDirectoryName(assembly.Location);
        var projectRoot = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(assemblyLocation!, "..", "..", "..", "..", "..", "src", "Adapters", "Adapter.gRPC"));
        var protosFolder = System.IO.Path.Combine(projectRoot, "Protos");

        // Act
        var protoFilesExist = System.IO.Directory.Exists(protosFolder) &&
                              System.IO.Directory.GetFiles(protosFolder, "*.proto").Length > 0;

        // Assert
        Assert.That(protoFilesExist, Is.True,
            $"Proto files should exist in {protosFolder} folder");
    }

    [Test]
    public void GrpcAdapter_Should_OnlyUseInputPorts()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(ClubExample.Adapter.gRPC.Services.MemberGrpcService).Assembly)
            .That()
            .ResideInNamespace($"{AdapterGrpcNamespace}.Services")
            .Should()
            .NotHaveDependencyOn($"{CoreNamespace}.OutputPorts")
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            "gRPC Adapter (driving adapter) should only depend on InputPorts, not OutputPorts");
    }

    [Test]
    public void GrpcAdapter_Should_NotReferenceOtherAdapters()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(ClubExample.Adapter.gRPC.Services.MemberGrpcService).Assembly)
            .Should()
            .NotHaveDependencyOnAll(
                "ClubExample.Adapter.Api",
                "ClubExample.Adapter.PostgreSQL",
                "ClubExample.Adapter.InMemoryData"
            )
            .GetResult();

        // Assert
        Assert.That(result.IsSuccessful, Is.True,
            $"gRPC Adapter should be completely independent of other adapters. Violations: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }
}
