using OpenCode.Api.Constants;
using OpenCode.Api.Models;

namespace OpenCode.Api.Ingestion;

public class TelemetryContractMapper
{
    private readonly TokenTypeValidator _tokenTypeValidator;
    private readonly string _studentKey;

    public TelemetryContractMapper(TokenTypeValidator tokenTypeValidator, string studentKey)
    {
        _tokenTypeValidator = tokenTypeValidator;
        _studentKey = studentKey;
    }

    public List<TokenUsageMetricRecord> MapMetrics(OtlpMetricRequest request, string sourceTransport)
    {
        var records = new List<TokenUsageMetricRecord>();

        if (request.ResourceMetrics is null)
            return records;

        foreach (var resourceMetric in request.ResourceMetrics)
        {
            if (resourceMetric.ScopeMetrics is null)
                continue;

            foreach (var scopeMetric in resourceMetric.ScopeMetrics)
            {
                if (scopeMetric.Metrics is null)
                    continue;

                foreach (var metric in scopeMetric.Metrics)
                {
                    if (metric.Name != OpenCodeSignals.MetricNameTokenUsage)
                        continue;

                    if (metric.Sum?.DataPoints is null)
                        continue;

                    foreach (var dataPoint in metric.Sum.DataPoints)
                    {
                        var typeAttribute = dataPoint.Attributes?
                            .FirstOrDefault(a => a.Key == "type");

                        var tokenType = typeAttribute?.Value?.StringValue ?? string.Empty;

                        if (!_tokenTypeValidator.IsValid(tokenType))
                            continue;

                        var cumulativeValue = ParseCumulativeValue(dataPoint);
                        var timestamp = ParseTimestamp(dataPoint.TimeUnixNano);

                        var rawAttributes = new Dictionary<string, object>();
                        if (dataPoint.Attributes is not null)
                        {
                            foreach (var attr in dataPoint.Attributes)
                            {
                                rawAttributes[attr.Key ?? ""] = attr.Value?.StringValue
                                    ?? attr.Value?.IntValue
                                    ?? attr.Value?.DoubleValue
                                    ?? (object?)attr.Value?.BoolValue
                                    ?? "";
                            }
                        }

                        records.Add(new TokenUsageMetricRecord
                        {
                            student_key = _studentKey,
                            metric_name = OpenCodeSignals.MetricNameTokenUsage,
                            token_type = tokenType,
                            cumulative_value = cumulativeValue,
                            unit = OpenCodeSignals.UnitTokens,
                            sample_timestamp_utc = timestamp,
                            source_transport = sourceTransport,
                            raw_attributes = rawAttributes,
                        });
                    }
                }
            }
        }

        return records;
    }

    public List<OpenCodeLogRecord> MapLogs(OtlpLogRequest request, string sourceTransport)
    {
        var records = new List<OpenCodeLogRecord>();

        if (request.ResourceLogs is null)
            return records;

        foreach (var resourceLog in request.ResourceLogs)
        {
            if (resourceLog.ScopeLogs is null)
                continue;

            foreach (var scopeLog in resourceLog.ScopeLogs)
            {
                if (scopeLog.LogRecords is null)
                    continue;

                foreach (var logRecord in scopeLog.LogRecords)
                {
                    var eventName = ExtractEventName(logRecord);

                    if (!OpenCodeSignals.SupportedLogEventSet.Contains(eventName))
                        continue;

                    var timestamp = ParseTimestamp(logRecord.TimeUnixNano);
                    var attributes = new Dictionary<string, object>();
                    if (logRecord.Attributes is not null)
                    {
                        foreach (var attr in logRecord.Attributes)
                        {
                            attributes[attr.Key ?? ""] = attr.Value?.StringValue
                                ?? attr.Value?.IntValue
                                ?? attr.Value?.DoubleValue
                                ?? (object?)attr.Value?.BoolValue
                                ?? "";
                        }
                    }

                    records.Add(new OpenCodeLogRecord
                    {
                        student_key = _studentKey,
                        event_name = eventName,
                        event_timestamp_utc = timestamp,
                        severity_text = logRecord.SeverityText,
                        trace_id = logRecord.TraceId,
                        span_id = logRecord.SpanId,
                        body = logRecord.Body?.StringValue,
                        attributes = attributes,
                    });
                }
            }
        }

        return records;
    }

    private static string ExtractEventName(OtlpLogRecord logRecord)
    {
        if (logRecord.Attributes is not null)
        {
            var eventAttr = logRecord.Attributes
                .FirstOrDefault(a => a.Key == "event.name" || a.Key == "event");
            if (eventAttr?.Value?.StringValue is not null)
                return eventAttr.Value.StringValue;
        }

        if (logRecord.Body?.StringValue is not null)
            return logRecord.Body.StringValue;

        return string.Empty;
    }

    private static long ParseCumulativeValue(OtlpDataPoint dataPoint)
    {
        if (dataPoint.AsInt is not null &&
            long.TryParse(dataPoint.AsInt, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var intVal))
        {
            return intVal;
        }

        if (dataPoint.AsDouble.HasValue)
            return (long)dataPoint.AsDouble.Value;

        return 0;
    }

    private static DateTime ParseTimestamp(string? timeUnixNano)
    {
        if (string.IsNullOrEmpty(timeUnixNano))
            return DateTime.UtcNow;

        if (long.TryParse(timeUnixNano,
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out var nanoSeconds))
        {
            var ticks = nanoSeconds / 100;
            var dateTime = new DateTime(ticks, DateTimeKind.Utc);
            if (dateTime.Year > 1900)
                return dateTime;
        }

        return DateTime.UtcNow;
    }
}
