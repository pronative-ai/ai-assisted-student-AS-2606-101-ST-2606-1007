using Microsoft.Azure.Cosmos;
using OpenCode.Api.Configuration;
using OpenCode.Api.Models;

namespace OpenCode.Api.Persistence;

public class CosmosTelemetryRepository : ICosmosTelemetryRepository
{
    private readonly Container _metricsContainer;
    private readonly Container _logsContainer;

    public CosmosTelemetryRepository(CosmosClient cosmosClient, ServiceConfiguration config)
    {
        var database = cosmosClient.GetDatabase(config.CosmosDatabaseId);
        _metricsContainer = database.GetContainer(config.CosmosMetricsContainerId);
        _logsContainer = database.GetContainer(config.CosmosLogsContainerId);
    }

    public async Task StoreMetricRecordAsync(TokenUsageMetricRecord record)
    {
        await _metricsContainer.CreateItemAsync(
            record,
            new PartitionKey(record.student_key));
    }

    public async Task StoreLogRecordAsync(OpenCodeLogRecord record)
    {
        await _logsContainer.CreateItemAsync(
            record,
            new PartitionKey(record.student_key));
    }

    public async Task<List<TokenUsageMetricRecord>> GetMetricRecordsAsync(
        string studentKey, string metricName, string tokenType,
        DateTime startTime, DateTime endTime)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.student_key = @studentKey " +
            "AND c.metric_name = @metricName " +
            "AND c.token_type = @tokenType " +
            "AND c.sample_timestamp_utc >= @startTime " +
            "AND c.sample_timestamp_utc <= @endTime " +
            "ORDER BY c.sample_timestamp_utc ASC")
            .WithParameter("@studentKey", studentKey)
            .WithParameter("@metricName", metricName)
            .WithParameter("@tokenType", tokenType)
            .WithParameter("@startTime", startTime)
            .WithParameter("@endTime", endTime);

        var results = new List<TokenUsageMetricRecord>();
        using var iterator = _metricsContainer.GetItemQueryIterator<TokenUsageMetricRecord>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<TokenUsageMetricRecord?> GetLatestSampleAtOrBeforeAsync(
        string studentKey, string metricName, string tokenType, DateTime beforeTime)
    {
        var query = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.student_key = @studentKey " +
            "AND c.metric_name = @metricName " +
            "AND c.token_type = @tokenType " +
            "AND c.sample_timestamp_utc <= @beforeTime " +
            "ORDER BY c.sample_timestamp_utc DESC")
            .WithParameter("@studentKey", studentKey)
            .WithParameter("@metricName", metricName)
            .WithParameter("@tokenType", tokenType)
            .WithParameter("@beforeTime", beforeTime);

        using var iterator = _metricsContainer.GetItemQueryIterator<TokenUsageMetricRecord>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<TokenUsageMetricRecord?> GetEarliestSampleInWindowAsync(
        string studentKey, string metricName, string tokenType,
        DateTime windowStart, DateTime windowEnd)
    {
        var query = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.student_key = @studentKey " +
            "AND c.metric_name = @metricName " +
            "AND c.token_type = @tokenType " +
            "AND c.sample_timestamp_utc >= @windowStart " +
            "AND c.sample_timestamp_utc <= @windowEnd " +
            "ORDER BY c.sample_timestamp_utc ASC")
            .WithParameter("@studentKey", studentKey)
            .WithParameter("@metricName", metricName)
            .WithParameter("@tokenType", tokenType)
            .WithParameter("@windowStart", windowStart)
            .WithParameter("@windowEnd", windowEnd);

        using var iterator = _metricsContainer.GetItemQueryIterator<TokenUsageMetricRecord>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }
}
