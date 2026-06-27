using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using OpenCode.Api.Configuration;
using OpenCode.Api.Endpoints;
using OpenCode.Api.Observability;

var builder = WebApplication.CreateBuilder(args);

var serviceConfig = builder.Configuration.BindServiceConfiguration();

if (serviceConfig.UseKeyVault && serviceConfig.KeyVaultUri is not null)
{
    var keyVaultClient = new SecretClient(serviceConfig.KeyVaultUri, new DefaultAzureCredential());
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

var app = builder.Build();

app.MapHealthEndpoints();

app.Run();
