using OpenCode.Api.Models;
using OpenCode.Api.Persistence;

namespace OpenCode.Api.Tests;

public class InMemoryTelemetryRepository : ICosmosTelemetryRepository
{
    private readonly List<TokenUsageMetricRecord> _metrics = new();
    private readonly List<OpenCodeLogRecord> _logs = new();

    public void Seed(TokenUsageMetricRecord record) => _metrics.Add(record);
    public void Clear() { _metrics.Clear(); _logs.Clear(); }

    public Task StoreMetricRecordAsync(TokenUsageMetricRecord record)
    {
        _metrics.Add(record);
        return Task.CompletedTask;
    }

    public Task StoreLogRecordAsync(OpenCodeLogRecord record)
    {
        _logs.Add(record);
        return Task.CompletedTask;
    }

    public Task<List<TokenUsageMetricRecord>> GetMetricRecordsAsync(
        string studentKey, string metricName, string tokenType,
        DateTime startTime, DateTime endTime)
    {
        var results = _metrics
            .Where(m => m.student_key == studentKey
                     && m.metric_name == metricName
                     && m.token_type == tokenType
                     && m.sample_timestamp_utc >= startTime
                     && m.sample_timestamp_utc <= endTime)
            .OrderBy(m => m.sample_timestamp_utc)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<TokenUsageMetricRecord?> GetLatestSampleAtOrBeforeAsync(
        string studentKey, string metricName, string tokenType, DateTime beforeTime)
    {
        var result = _metrics
            .Where(m => m.student_key == studentKey
                     && m.metric_name == metricName
                     && m.token_type == tokenType
                     && m.sample_timestamp_utc <= beforeTime)
            .OrderByDescending(m => m.sample_timestamp_utc)
            .FirstOrDefault();
        return Task.FromResult(result);
    }

    public Task<TokenUsageMetricRecord?> GetEarliestSampleInWindowAsync(
        string studentKey, string metricName, string tokenType,
        DateTime windowStart, DateTime windowEnd)
    {
        var result = _metrics
            .Where(m => m.student_key == studentKey
                     && m.metric_name == metricName
                     && m.token_type == tokenType
                     && m.sample_timestamp_utc >= windowStart
                     && m.sample_timestamp_utc <= windowEnd)
            .OrderBy(m => m.sample_timestamp_utc)
            .FirstOrDefault();
        return Task.FromResult(result);
    }
}
