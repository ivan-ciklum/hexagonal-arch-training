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
            .Should()
            .BeInterfaces()
            .And()
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
            .Should()
            .MeetCustomRule(rule)
            .GetResult();

        Assert.That(result.IsSuccessful, $"The following interfaces have no corresponding implementation (ISomeUseCase, SomeUseCase) : {string.Join(", ", result?.FailingTypeNames ?? [])}");
    }
}
