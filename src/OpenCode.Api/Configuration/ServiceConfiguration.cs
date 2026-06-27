namespace OpenCode.Api.Configuration;

public class ServiceConfiguration
{
    public const string SectionName = "Service";

    public string StudentKey { get; init; } = string.Empty;
    public string CosmosConnectionString { get; init; } = string.Empty;
    public string CosmosDatabaseId { get; init; } = "opencode-telemetry";
    public string CosmosMetricsContainerId { get; init; } = "token-usage-metrics";
    public string CosmosLogsContainerId { get; init; } = "opencode-logs";
    public bool UseKeyVault { get; init; } = false;
    public Uri? KeyVaultUri { get; init; }
}
