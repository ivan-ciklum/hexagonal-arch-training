using ClubExample.Core.Domain;

namespace Architecture.UnitTest;

[TestFixture]
public class SolutionTypes
{
    public static readonly string CoreNamespace = "ClubExample.Core";
    public static readonly string CoreInputPortsNamespace = "ClubExample.Core.InputPorts";
    public static readonly string CoreOutputPortsNamespace = "ClubExample.Core.OutputPorts";
    public static readonly string CoreUseCasesNamespace = "ClubExample.Core.UseCases";
    public static readonly string AdapterApiNamespace = "ClubExample.Adapter.Api";
    public static readonly string AdapterPostgreSQLNamespace = "ClubExample.Adapter.PostgreSQL";

    public static readonly System.Reflection.Assembly CoreAssembly = typeof(Club).Assembly;
    public static readonly System.Reflection.Assembly AdapterApiAssembly = typeof(ClubExample.Adapter.Api.IMarkInterface).Assembly;
    public static readonly System.Reflection.Assembly AdapterPostgreSQLAssembly = typeof(ClubExample.Adapter.PostgreSQL.OneRepository).Assembly;
}
