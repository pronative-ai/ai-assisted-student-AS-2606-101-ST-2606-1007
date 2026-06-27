namespace OpenCode.Api.Filters;

public class IngestionExceptionFilter : IEndpointFilter
{
    private readonly ILogger<IngestionExceptionFilter> _logger;

    public IngestionExceptionFilter(ILogger<IngestionExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ingestion error occurred on {Path}",
                context.HttpContext.Request.Path);

            return Results.Problem(
                statusCode: 500,
                title: "Ingestion Error",
                detail: "An unexpected error occurred during telemetry ingestion. " +
                        "The payload may be malformed or incomplete.");
        }
    }
}
