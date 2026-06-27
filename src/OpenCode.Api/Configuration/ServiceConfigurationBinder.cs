namespace OpenCode.Api.Configuration;

public static class ServiceConfigurationBinder
{
    public static ServiceConfiguration BindServiceConfiguration(this IConfiguration configuration)
    {
        var section = configuration.GetSection(ServiceConfiguration.SectionName);
        var config = new ServiceConfiguration
        {
            StudentKey = section["StudentKey"] ?? string.Empty,
            CosmosConnectionString = section["CosmosConnectionString"] ?? string.Empty,
            CosmosDatabaseId = section["CosmosDatabaseId"] ?? "opencode-telemetry",
            CosmosMetricsContainerId = section["CosmosMetricsContainerId"] ?? "token-usage-metrics",
            CosmosLogsContainerId = section["CosmosLogsContainerId"] ?? "opencode-logs",
            UseKeyVault = bool.TryParse(section["UseKeyVault"], out var useKv) && useKv,
            KeyVaultUri = Uri.TryCreate(section["KeyVaultUri"], UriKind.Absolute, out var kvUri) ? kvUri : null,
        };
        return config;
    }
}
