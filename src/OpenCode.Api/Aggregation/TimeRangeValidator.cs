namespace OpenCode.Api.Aggregation;

public class TimeRangeValidationResult
{
    public bool IsValid { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; } = 400;
}

public class TimeRangeValidator
{
    public TimeRangeValidationResult Validate(string? startTime, string? endTime)
    {
        if (string.IsNullOrWhiteSpace(startTime))
            return Error("Missing required parameter: start_time.");

        if (string.IsNullOrWhiteSpace(endTime))
            return Error("Missing required parameter: end_time.");

        if (!DateTime.TryParse(startTime, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var start))
            return Error("Invalid start_time format. Use ISO 8601 UTC (e.g., 2026-01-01T10:00:00Z).");

        if (!DateTime.TryParse(endTime, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var end))
            return Error("Invalid end_time format. Use ISO 8601 UTC (e.g., 2026-01-01T10:00:00Z).");

        if (start.Kind == DateTimeKind.Unspecified)
            start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        if (end.Kind == DateTimeKind.Unspecified)
            end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        if (start > end)
            return Error("start_time must not be after end_time.");

        return new TimeRangeValidationResult
        {
            IsValid = true,
            StartTime = start,
            EndTime = end,
        };
    }

    private static TimeRangeValidationResult Error(string message)
    {
        return new TimeRangeValidationResult
        {
            IsValid = false,
            ErrorMessage = message,
        };
    }
}
