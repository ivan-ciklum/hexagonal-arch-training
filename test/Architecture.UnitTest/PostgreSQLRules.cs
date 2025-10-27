using NetArchTest.Rules;

namespace Architecture.UnitTest;

public class PostgreSQLRules
{
    [Test]
    public void Adapter_PostgreSQL_Should_Not_Depend_On_Adapter_API()
    {
        var result = Types.InAssembly(SolutionTypes.AdapterPostgreSQLAssembly)
                          .Should()
                          .NotHaveDependencyOn(SolutionTypes.AdapterApiNamespace)
                          .GetResult();

        Assert.That(result.IsSuccessful, $"Error: Adapter PostgreSQL should not depend on api adapters");
    }

    [Test]
    public void Adapter_PostgreSQL_must_have_reference_to_Core()
    {
        var result = Types.InAssembly(SolutionTypes.AdapterPostgreSQLAssembly)
                          .Should()
                          .HaveDependencyOn(SolutionTypes.CoreOutputPortsNamespace)
                          .GetResult();

        Assert.That(result.IsSuccessful, $"Error: Adapter PostgreSQL must have a reference to Core.");
    }

}
