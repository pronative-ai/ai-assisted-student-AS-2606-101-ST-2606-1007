using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using OpenCode.Api.Configuration;
using OpenCode.Api.Endpoints;
using OpenCode.Api.Filters;
using OpenCode.Api.Ingestion;
using OpenCode.Api.Observability;
using OpenCode.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

var serviceConfig = builder.Configuration.BindServiceConfiguration();

if (serviceConfig.UseKeyVault && serviceConfig.KeyVaultUri is not null)
{
    builder.Configuration.AddAzureKeyVault(serviceConfig.KeyVaultUri, new DefaultAzureCredential());
}

builder.Services.AddOpenCodeObservability(builder.Configuration, serviceConfig);

builder.Services.AddSingleton(serviceConfig);

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<ServiceConfiguration>();
    return new CosmosClient(
        connectionString: config.CosmosConnectionString);
});

builder.Services.AddSingleton<TokenTypeValidator>();
builder.Services.AddSingleton<TelemetryContractMapper>(sp =>
{
    var validator = sp.GetRequiredService<TokenTypeValidator>();
    var config = sp.GetRequiredService<ServiceConfiguration>();
    return new TelemetryContractMapper(validator, config.StudentKey);
});
builder.Services.AddSingleton<ICosmosTelemetryRepository, CosmosTelemetryRepository>();
builder.Services.AddSingleton<IngestionExceptionFilter>();

var app = builder.Build();

app.MapHealthEndpoints();
app.MapOtlpMetricsEndpoint();
app.MapOtlpLogsEndpoint();

app.Run();
