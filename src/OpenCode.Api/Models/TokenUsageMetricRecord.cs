namespace OpenCode.Api.Models;

public class TokenUsageMetricRecord
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string record_type { get; set; } = "token_usage_metric";
    public string student_key { get; set; } = string.Empty;
    public string metric_name { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
    public long cumulative_value { get; set; }
    public string unit { get; set; } = string.Empty;
    public DateTime sample_timestamp_utc { get; set; }
    public DateTime ingested_at_utc { get; set; } = DateTime.UtcNow;
    public string source_transport { get; set; } = string.Empty;
    public Dictionary<string, object>? raw_attributes { get; set; }
}
