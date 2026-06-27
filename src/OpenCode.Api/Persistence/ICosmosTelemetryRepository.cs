using OpenCode.Api.Models;

namespace OpenCode.Api.Persistence;

public interface ICosmosTelemetryRepository
{
    Task StoreMetricRecordAsync(TokenUsageMetricRecord record);
    Task StoreLogRecordAsync(OpenCodeLogRecord record);
    Task<List<TokenUsageMetricRecord>> GetMetricRecordsAsync(
        string studentKey, string metricName, string tokenType,
        DateTime startTime, DateTime endTime);
    Task<TokenUsageMetricRecord?> GetLatestSampleAtOrBeforeAsync(
        string studentKey, string metricName, string tokenType, DateTime beforeTime);
    Task<TokenUsageMetricRecord?> GetEarliestSampleInWindowAsync(
        string studentKey, string metricName, string tokenType,
        DateTime windowStart, DateTime windowEnd);
}
