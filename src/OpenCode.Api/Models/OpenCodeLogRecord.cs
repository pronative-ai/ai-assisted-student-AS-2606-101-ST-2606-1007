namespace OpenCode.Api.Models;

public class OpenCodeLogRecord
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string record_type { get; set; } = "opencode_log";
    public string student_key { get; set; } = string.Empty;
    public string event_name { get; set; } = string.Empty;
    public DateTime event_timestamp_utc { get; set; }
    public DateTime ingested_at_utc { get; set; } = DateTime.UtcNow;
    public string? severity_text { get; set; }
    public string? trace_id { get; set; }
    public string? span_id { get; set; }
    public string? body { get; set; }
    public Dictionary<string, object>? attributes { get; set; }
}
