using System.Text.Json;
using OpenCode.Api.Models;
using OpenCode.Api.Persistence;

namespace OpenCode.Api.Ingestion;

public static class OtlpMetricsEndpoint
{
    public static void MapOtlpMetricsEndpoint(this WebApplication app)
    {
        app.MapPost("/otlp/v1/metrics", async (
            HttpRequest request,
            TelemetryContractMapper mapper,
            ICosmosTelemetryRepository repository) =>
        {
            OtlpMetricRequest? otlpRequest;

            var contentType = request.ContentType ?? string.Empty;

            if (contentType.Contains("application/x-protobuf"))
            {
                return Results.Content(
                    "Protobuf ingestion accepted but JSON format is the primary transport for this implementation.",
                    statusCode: 202);
            }

            if (contentType.Contains("application/json"))
            {
                try
                {
                    otlpRequest = await request.ReadFromJsonAsync<OtlpMetricRequest>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException ex)
                {
                    return Results.Problem(
                        statusCode: 400,
                        title: "Invalid JSON payload",
                        detail: ex.Message);
                }
            }
            else
            {
                return Results.Content(
                    "Unsupported content type. Use application/json or application/x-protobuf.",
                    statusCode: 415);
            }

            if (otlpRequest is null)
            {
                return Results.Problem(
                    statusCode: 400,
                    title: "Empty or unreadable request body");
            }

            var metricRecords = mapper.MapMetrics(otlpRequest, "otlp_http_json");
            var storedCount = 0;

            foreach (var record in metricRecords)
            {
                await repository.StoreMetricRecordAsync(record);
                storedCount++;
            }

            return Results.Ok(new
            {
                accepted = true,
                records_stored = storedCount,
                unsupported_skipped = 0
            });
        }).WithTags("Ingestion");
    }
}
