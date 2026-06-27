using Xunit;
using OpenCode.Api.Ingestion;
using OpenCode.Api.Models;

namespace OpenCode.Api.Tests;

public class TelemetryContractMapperTests
{
    private readonly TelemetryContractMapper _mapper;
    private const string TestStudentKey = "student-dev-1007";

    public TelemetryContractMapperTests()
    {
        var validator = new TokenTypeValidator();
        _mapper = new TelemetryContractMapper(validator, TestStudentKey);
    }

    [Fact]
    public void MapMetrics_SupportedTypes_ReturnsRecords()
    {
        var request = CreateMetricRequest("input", "1200");

        var records = _mapper.MapMetrics(request, "otlp_http_json");

        Assert.Single(records);
        var record = records[0];
        Assert.Equal("opencode.token.usage", record.metric_name);
        Assert.Equal("input", record.token_type);
        Assert.Equal(1200, record.cumulative_value);
        Assert.Equal(TestStudentKey, record.student_key);
        Assert.Equal("tokens", record.unit);
    }

    [Fact]
    public void MapMetrics_UnsupportedTokenType_ReturnsEmpty()
    {
        var request = CreateMetricRequest("unsupportedType", "500");

        var records = _mapper.MapMetrics(request, "otlp_http_json");

        Assert.Empty(records);
    }

    [Fact]
    public void MapMetrics_AllSupportedTypes_ReturnsAllRecords()
    {
        var request = new OtlpMetricRequest
        {
            ResourceMetrics = new List<ResourceMetric>
            {
                new ResourceMetric
                {
                    ScopeMetrics = new List<ScopeMetric>
                    {
                        new ScopeMetric
                        {
                            Metrics = new List<OtlpMetric>
                            {
                                CreateMetric("input", "100"),
                                CreateMetric("output", "200"),
                                CreateMetric("reasoning", "300"),
                                CreateMetric("cacheRead", "400"),
                                CreateMetric("cacheCreation", "500"),
                            }
                        }
                    }
                }
            }
        };

        var records = _mapper.MapMetrics(request, "otlp_http_json");

        Assert.Equal(5, records.Count);
        Assert.Contains(records, r => r.token_type == "input" && r.cumulative_value == 100);
        Assert.Contains(records, r => r.token_type == "output" && r.cumulative_value == 200);
        Assert.Contains(records, r => r.token_type == "reasoning" && r.cumulative_value == 300);
        Assert.Contains(records, r => r.token_type == "cacheRead" && r.cumulative_value == 400);
        Assert.Contains(records, r => r.token_type == "cacheCreation" && r.cumulative_value == 500);
    }

    [Fact]
    public void MapMetrics_WrongMetricName_ReturnsEmpty()
    {
        var request = new OtlpMetricRequest
        {
            ResourceMetrics = new List<ResourceMetric>
            {
                new ResourceMetric
                {
                    ScopeMetrics = new List<ScopeMetric>
                    {
                        new ScopeMetric
                        {
                            Metrics = new List<OtlpMetric>
                            {
                                new OtlpMetric
                                {
                                    Name = "some.other.metric",
                                    Sum = new OtlpSum
                                    {
                                        DataPoints = new List<OtlpDataPoint>
                                        {
                                            new OtlpDataPoint
                                            {
                                                Attributes = new List<OtlpAttribute>
                                                {
                                                    new() { Key = "type", Value = new OtlpAttributeValue { StringValue = "input" } }
                                                },
                                                AsInt = "100",
                                                TimeUnixNano = "1704067200000000000"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var records = _mapper.MapMetrics(request, "otlp_http_json");

        Assert.Empty(records);
    }

    [Fact]
    public void MapLogs_SupportedEvents_ReturnsRecords()
    {
        var request = CreateLogRequest("api_request", "Request processed");

        var records = _mapper.MapLogs(request, "otlp_http_json");

        Assert.Single(records);
        var record = records[0];
        Assert.Equal("api_request", record.event_name);
        Assert.Equal(TestStudentKey, record.student_key);
        Assert.Equal("Request processed", record.body);
    }

    [Fact]
    public void MapLogs_ApiError_ReturnsRecord()
    {
        var request = CreateLogRequest("api_error", "Error occurred");

        var records = _mapper.MapLogs(request, "otlp_http_json");

        Assert.Single(records);
        Assert.Equal("api_error", records[0].event_name);
        Assert.Equal("Error occurred", records[0].body);
    }

    [Fact]
    public void MapLogs_UnsupportedEvent_ReturnsEmpty()
    {
        var request = CreateLogRequest("unsupported_event", "Some log");

        var records = _mapper.MapLogs(request, "otlp_http_json");

        Assert.Empty(records);
    }

    private static OtlpMetricRequest CreateMetricRequest(string tokenType, string value)
    {
        return new OtlpMetricRequest
        {
            ResourceMetrics = new List<ResourceMetric>
            {
                new ResourceMetric
                {
                    ScopeMetrics = new List<ScopeMetric>
                    {
                        new ScopeMetric
                        {
                            Metrics = new List<OtlpMetric>
                            {
                                new OtlpMetric
                                {
                                    Name = "opencode.token.usage",
                                    Sum = new OtlpSum
                                    {
                                        DataPoints = new List<OtlpDataPoint>
                                        {
                                            new OtlpDataPoint
                                            {
                                                Attributes = new List<OtlpAttribute>
                                                {
                                                    new() { Key = "type", Value = new OtlpAttributeValue { StringValue = tokenType } }
                                                },
                                                AsInt = value,
                                                TimeUnixNano = "1704067200000000000"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static OtlpMetric CreateMetric(string tokenType, string value)
    {
        return new OtlpMetric
        {
            Name = "opencode.token.usage",
            Sum = new OtlpSum
            {
                DataPoints = new List<OtlpDataPoint>
                {
                    new OtlpDataPoint
                    {
                        Attributes = new List<OtlpAttribute>
                        {
                            new() { Key = "type", Value = new OtlpAttributeValue { StringValue = tokenType } }
                        },
                        AsInt = value,
                        TimeUnixNano = "1704067200000000000"
                    }
                }
            }
        };
    }

    private static OtlpLogRequest CreateLogRequest(string eventName, string body)
    {
        return new OtlpLogRequest
        {
            ResourceLogs = new List<ResourceLog>
            {
                new ResourceLog
                {
                    ScopeLogs = new List<ScopeLog>
                    {
                        new ScopeLog
                        {
                            LogRecords = new List<OtlpLogRecord>
                            {
                                new OtlpLogRecord
                                {
                                    TimeUnixNano = "1704067200000000000",
                                    Body = new OtlpLogBody { StringValue = body },
                                    Attributes = new List<OtlpAttribute>
                                    {
                                        new() { Key = "event.name", Value = new OtlpAttributeValue { StringValue = eventName } }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
