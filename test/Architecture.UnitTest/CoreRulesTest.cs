using NetArchTest.Rules;

namespace Architecture.UnitTest;

public class CoreRulesTest
{
    [Test]
    public void Core_Should_Not_Depend_On_Adapters()
    {
        var result = Types.InAssembly(SolutionTypes.CoreAssembly)
                          .Should()
                          .NotHaveDependencyOn(SolutionTypes.AdapterApiNamespace)
                          .And()
                          .NotHaveDependencyOn(SolutionTypes.AdapterPostgreSQLNamespace)
                          .GetResult();

        Assert.That(result.IsSuccessful, $"Error: Core should not depend on any adapters");
    }

    [Test]
    public void All_Interfaces_In_Input_Ports_Must_End_With_UseCase_Suffix()
    {
        var result = Types.InAssembly(SolutionTypes.CoreAssembly)
            .That()
            .ResideInNamespace(SolutionTypes.CoreInputPortsNamespace)
            .And()
            .AreInterfaces()
            .Should()
            .HaveNameEndingWith("UseCase")
            .GetResult();

        Assert.That(result.IsSuccessful,$"The following interfaces not match the rule: {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void All_UseCases_In_Input_Ports_Should_Be_Implemented_In_UseCases_Namespace()
    {
        var rule = new HasMatchingImplementationRule(SolutionTypes.CoreAssembly, SolutionTypes.CoreUseCasesNamespace, "UseCase");

        var result = Types.InAssembly(SolutionTypes.CoreAssembly)
            .That().ResideInNamespace(SolutionTypes.CoreInputPortsNamespace)
            .And()
            .AreInterfaces()
            .Should()
            .MeetCustomRule(rule)
            .GetResult();

        Assert.That(result.IsSuccessful, $"The following interfaces have no corresponding implementation (ISomeUseCase, SomeUseCase) : {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }

    [Test]
    public void Repository_Interfaces_Should_Only_Expose_Methods_Starting_With_Get()
    {
        // Get all repository interfaces from Core.OutputPorts
        // Exclude ICacheRepository as it's a special type that requires write/delete operations
        var repositoryInterfaces = Types.InAssembly(SolutionTypes.CoreAssembly)
            .That()
            .ResideInNamespace(SolutionTypes.CoreOutputPortsNamespace)
            .And()
            .AreInterfaces()
            .And()
            .HaveNameEndingWith("Repository")
            .GetTypes()
            .Where(t => t.Name != "ICacheRepository"); // Cache repositories need Set/Remove operations

        var violatingMethods = new List<string>();

        foreach (var repositoryInterface in repositoryInterfaces)
        {
            var methods = repositoryInterface.GetMethods()
                .Where(m => !m.IsSpecialName); // Exclude property getters/setters

            foreach (var method in methods)
            {
                if (!method.Name.StartsWith("Get", StringComparison.Ordinal))
                {
                    violatingMethods.Add($"{repositoryInterface.Name}.{method.Name}");
                }
            }
        }

        Assert.That(violatingMethods, Is.Empty,
            $"Repository interfaces (except ICacheRepository) should only expose methods that begin with 'Get'. " +
            $"Violating methods: {string.Join(", ", violatingMethods)}");
    }
}
