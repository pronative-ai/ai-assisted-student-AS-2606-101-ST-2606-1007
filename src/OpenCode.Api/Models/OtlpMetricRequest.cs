using System.Text.Json.Serialization;

namespace OpenCode.Api.Models;

public class OtlpMetricRequest
{
    [JsonPropertyName("resourceMetrics")]
    public List<ResourceMetric>? ResourceMetrics { get; set; }
}

public class ResourceMetric
{
    [JsonPropertyName("scopeMetrics")]
    public List<ScopeMetric>? ScopeMetrics { get; set; }
}

public class ScopeMetric
{
    [JsonPropertyName("metrics")]
    public List<OtlpMetric>? Metrics { get; set; }
}

public class OtlpMetric
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("sum")]
    public OtlpSum? Sum { get; set; }
}

public class OtlpSum
{
    [JsonPropertyName("dataPoints")]
    public List<OtlpDataPoint>? DataPoints { get; set; }

    [JsonPropertyName("aggregationTemporality")]
    public int AggregationTemporality { get; set; }

    [JsonPropertyName("isMonotonic")]
    public bool IsMonotonic { get; set; }
}

public class OtlpDataPoint
{
    [JsonPropertyName("attributes")]
    public List<OtlpAttribute>? Attributes { get; set; }

    [JsonPropertyName("asInt")]
    public string? AsInt { get; set; }

    [JsonPropertyName("asDouble")]
    public double? AsDouble { get; set; }

    [JsonPropertyName("timeUnixNano")]
    public string? TimeUnixNano { get; set; }
}

public class OtlpAttribute
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public OtlpAttributeValue? Value { get; set; }
}

public class OtlpAttributeValue
{
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }

    [JsonPropertyName("intValue")]
    public string? IntValue { get; set; }

    [JsonPropertyName("doubleValue")]
    public double? DoubleValue { get; set; }

    [JsonPropertyName("boolValue")]
    public bool? BoolValue { get; set; }
}
