using Microsoft.Azure.Cosmos;
using OpenCode.Api.Configuration;

namespace OpenCode.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithTags("Health");

        app.MapGet("/ready", async (CosmosClient cosmosClient, ServiceConfiguration config) =>
        {
            try
            {
                var database = cosmosClient.GetDatabase(config.CosmosDatabaseId);
                await database.ReadAsync();
                return Results.Ok(new { status = "ready" });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 503,
                    title: "Service Not Ready",
                    detail: ex.Message);
            }
        }).WithTags("Health");
    }
}
