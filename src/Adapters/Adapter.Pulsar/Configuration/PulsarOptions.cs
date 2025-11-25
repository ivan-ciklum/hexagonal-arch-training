namespace ClubExample.Adapter.Pulsar.Configuration;

/// <summary>
/// Configuration options for Apache Pulsar connection.
/// </summary>
public sealed class PulsarOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Pulsar";

    /// <summary>
    /// Pulsar service URL (e.g., pulsar://localhost:6650)
    /// </summary>
    public string ServiceUrl { get; set; } = "pulsar://localhost:6650";

    /// <summary>
    /// Optional: Authentication token for secure connections
    /// </summary>
    public string? AuthenticationToken { get; set; }

    /// <summary>
    /// Optional: TLS configuration
    /// </summary>
    public bool EnableTls { get; set; } = false;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Operation timeout in seconds
    /// </summary>
    public int OperationTimeoutSeconds { get; set; } = 30;
}
