using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenCode.Api.Configuration;

namespace OpenCode.Api.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddOpenCodeObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceConfiguration serviceConfig)
    {
        var appInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
                                          ?? configuration["ApplicationInsights:ConnectionString"]
                                          ?? string.Empty;

        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            services.AddOpenTelemetry()
                .UseAzureMonitor(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;
                });
        }

        return services;
    }
}
