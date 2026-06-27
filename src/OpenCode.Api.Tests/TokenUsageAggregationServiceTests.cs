using Xunit;
using OpenCode.Api.Aggregation;
using OpenCode.Api.Models;

namespace OpenCode.Api.Tests;

public class TokenUsageAggregationServiceTests
{
    private readonly InMemoryTelemetryRepository _repo;
    private readonly TokenUsageAggregationService _service;
    private const string StudentKey = "student-dev-1007";
    private static readonly DateTime Start = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = Start.AddHours(1);

    public TokenUsageAggregationServiceTests()
    {
        _repo = new InMemoryTelemetryRepository();
        var selector = new BaselineSelector(_repo, StudentKey);
        _service = new TokenUsageAggregationService(selector);
    }

    [Fact]
    public async Task AggregateAsync_100To145_Returns45()
    {
        _repo.Seed(CreateMetric("output", 100, Start));
        _repo.Seed(CreateMetric("output", 145, End));

        var response = await _service.AggregateAsync(Start, End);

        Assert.Equal(45, response.totals.output);
        Assert.Equal("cumulative_delta", response.calculation_mode);
    }

    [Fact]
    public async Task AggregateAsync_BaselineBeforeWindow_ComputesCorrectDelta()
    {
        var beforeStart = Start.AddMinutes(-5);
        _repo.Seed(CreateMetric("reasoning", 80, beforeStart));
        _repo.Seed(CreateMetric("reasoning", 110, Start.AddMinutes(30)));

        var response = await _service.AggregateAsync(Start, Start.AddMinutes(30));

        Assert.Equal(30, response.totals.reasoning);
    }

    [Fact]
    public async Task AggregateAsync_AllSupportedTypes_ReturnsAllTotals()
    {
        foreach (var type in new[] { "input", "output", "reasoning", "cacheRead", "cacheCreation" })
        {
            _repo.Seed(CreateMetric(type, 100, Start));
            _repo.Seed(CreateMetric(type, 150, End));
        }

        var response = await _service.AggregateAsync(Start, End);

        Assert.Equal(50, response.totals.input);
        Assert.Equal(50, response.totals.output);
        Assert.Equal(50, response.totals.reasoning);
        Assert.Equal(50, response.totals.cacheRead);
        Assert.Equal(50, response.totals.cacheCreation);
    }

    [Fact]
    public async Task AggregateAsync_OnlySomeTypesHaveData_ReturnsZeroForOthers()
    {
        _repo.Seed(CreateMetric("input", 200, Start));
        _repo.Seed(CreateMetric("input", 250, End));
        _repo.Seed(CreateMetric("cacheRead", 50, Start));
        _repo.Seed(CreateMetric("cacheRead", 60, End));

        var response = await _service.AggregateAsync(Start, End);

        Assert.Equal(50, response.totals.input);
        Assert.Equal(10, response.totals.cacheRead);
        Assert.Equal(0, response.totals.output);
        Assert.Equal(0, response.totals.reasoning);
        Assert.Equal(0, response.totals.cacheCreation);
    }

    [Fact]
    public async Task AggregateAsync_NoData_ReturnsAllZeros()
    {
        var response = await _service.AggregateAsync(Start, End);

        Assert.Equal(0, response.totals.input);
        Assert.Equal(0, response.totals.output);
        Assert.Equal(0, response.totals.reasoning);
        Assert.Equal(0, response.totals.cacheRead);
        Assert.Equal(0, response.totals.cacheCreation);
    }

    [Fact]
    public async Task AggregateAsync_CounterDecrease_ReturnsZeroAndWarning()
    {
        _repo.Seed(CreateMetric("input", 200, Start));
        _repo.Seed(CreateMetric("input", 150, End));

        var response = await _service.AggregateAsync(Start, End);

        Assert.Equal(0, response.totals.input);
        Assert.Contains("Counter decrease", response.warnings[0]);
    }

    [Fact]
    public async Task AggregateAsync_IdenticalStartAndEnd_UsesSameSampleAsBaseline()
    {
        _repo.Seed(CreateMetric("input", 100, Start));

        var response = await _service.AggregateAsync(Start, Start);

        Assert.Equal(0, response.totals.input);
    }

    [Fact]
    public async Task AggregateAsync_SameBaselineAndClosing_ReturnsZero()
    {
        _repo.Seed(CreateMetric("input", 100, Start));
        _repo.Seed(CreateMetric("input", 100, End));

        var response = await _service.AggregateAsync(Start, End);

        Assert.Equal(0, response.totals.input);
    }

    private static TokenUsageMetricRecord CreateMetric(string tokenType, long value, DateTime timestamp)
    {
        return new TokenUsageMetricRecord
        {
            student_key = StudentKey,
            metric_name = "opencode.token.usage",
            token_type = tokenType,
            cumulative_value = value,
            sample_timestamp_utc = timestamp,
            unit = "tokens",
        };
    }
}
