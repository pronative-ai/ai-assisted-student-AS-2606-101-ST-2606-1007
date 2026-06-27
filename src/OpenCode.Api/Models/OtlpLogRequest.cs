using System.Text.Json.Serialization;

namespace OpenCode.Api.Models;

public class OtlpLogRequest
{
    [JsonPropertyName("resourceLogs")]
    public List<ResourceLog>? ResourceLogs { get; set; }
}

public class ResourceLog
{
    [JsonPropertyName("scopeLogs")]
    public List<ScopeLog>? ScopeLogs { get; set; }
}

public class ScopeLog
{
    [JsonPropertyName("logRecords")]
    public List<OtlpLogRecord>? LogRecords { get; set; }
}

public class OtlpLogRecord
{
    [JsonPropertyName("timeUnixNano")]
    public string? TimeUnixNano { get; set; }

    [JsonPropertyName("observedTimeUnixNano")]
    public string? ObservedTimeUnixNano { get; set; }

    [JsonPropertyName("severityText")]
    public string? SeverityText { get; set; }

    [JsonPropertyName("severityNumber")]
    public int? SeverityNumber { get; set; }

    [JsonPropertyName("body")]
    public OtlpLogBody? Body { get; set; }

    [JsonPropertyName("attributes")]
    public List<OtlpAttribute>? Attributes { get; set; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("spanId")]
    public string? SpanId { get; set; }
}

public class OtlpLogBody
{
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }
}
