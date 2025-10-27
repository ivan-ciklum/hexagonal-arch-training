using Mono.Cecil;
using NetArchTest.Rules;
using System.Reflection;

namespace Architecture.UnitTest;

public class HasMatchingImplementationRule : ICustomRule
{
    private readonly Assembly _implementationAssembly;
    private readonly string _implementationNamespace;
    private readonly string _suffix;

    public HasMatchingImplementationRule(Assembly implementationAssembly, string implementationNamespace, string suffix = "")
    {
        _implementationAssembly = implementationAssembly;
        _implementationNamespace = implementationNamespace;
        _suffix = suffix;
    }

    public bool MeetsRule(TypeDefinition type)
    {
        if (!type.IsInterface)
            return true;

        var expectedName = type.Name.EndsWith(_suffix)
            ? type.Name.Substring(1)
            : type.Name;

        var interfaceType = _implementationAssembly.GetType(type.FullName);
        if (interfaceType == null)
        {
            return false;
        }

        // Search one class that implements the interface
        var implementationExists = _implementationAssembly.GetTypes()
            .Where(t => t.Namespace == _implementationNamespace)
            .Any(t => t.IsClass
                        && !t.IsAbstract
                        && t.Name == expectedName
                        && interfaceType.IsAssignableFrom(t));

        return implementationExists;
    }


}

