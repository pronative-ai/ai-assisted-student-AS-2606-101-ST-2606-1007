using OpenCode.Api.Aggregation;

namespace OpenCode.Api.Endpoints;

public static class TokenUsageEndpoint
{
    public static void MapTokenUsageEndpoint(this WebApplication app)
    {
        app.MapGet("/api/opencode/token-usage", async (
            string? start_time,
            string? end_time,
            TimeRangeValidator validator,
            TokenUsageAggregationService aggregationService) =>
        {
            var validation = validator.Validate(start_time, end_time);

            if (!validation.IsValid)
            {
                return Results.Problem(
                    statusCode: validation.StatusCode,
                    title: "Invalid request",
                    detail: validation.ErrorMessage);
            }

            var response = await aggregationService.AggregateAsync(
                validation.StartTime, validation.EndTime);

            return Results.Ok(response);
        }).WithTags("Token Usage");
    }
}
