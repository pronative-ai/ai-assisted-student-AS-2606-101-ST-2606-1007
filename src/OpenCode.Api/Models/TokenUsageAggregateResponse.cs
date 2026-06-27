namespace OpenCode.Api.Models;

public class TokenUsageAggregateResponse
{
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
    public string calculation_mode { get; set; } = "cumulative_delta";
    public string baseline_policy { get; set; } =
        "use_latest_at_or_before_start_else_earliest_available_within_or_immediately_before_window";
    public TokenTotals totals { get; set; } = new();
    public List<string> warnings { get; set; } = new();
}

public class TokenTotals
{
    public long input { get; set; }
    public long output { get; set; }
    public long reasoning { get; set; }
    public long cacheRead { get; set; }
    public long cacheCreation { get; set; }
}
