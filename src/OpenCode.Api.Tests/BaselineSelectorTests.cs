using Xunit;
using OpenCode.Api.Aggregation;
using OpenCode.Api.Models;

namespace OpenCode.Api.Tests;

public class BaselineSelectorTests
{
    private readonly InMemoryTelemetryRepository _repo;
    private readonly BaselineSelector _selector;
    private const string StudentKey = "student-dev-1007";

    public BaselineSelectorTests()
    {
        _repo = new InMemoryTelemetryRepository();
        _selector = new BaselineSelector(_repo, StudentKey);
    }

    [Fact]
    public async Task SelectAsync_SampleAtStart_UsesExactBaseline()
    {
        var t = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        _repo.Seed(CreateMetric("output", 100, t));

        var result = await _selector.SelectAsync("output", t, t.AddHours(1));

        Assert.NotNull(result.Baseline);
        Assert.Equal(100, result.Baseline!.cumulative_value);
        Assert.False(result.UsedFallback);
    }

    [Fact]
    public async Task SelectAsync_SampleBeforeStart_UsesPriorSample()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var beforeStart = start.AddMinutes(-5);
        _repo.Seed(CreateMetric("output", 100, beforeStart));

        var result = await _selector.SelectAsync("output", start, start.AddHours(1));

        Assert.NotNull(result.Baseline);
        Assert.Equal(100, result.Baseline!.cumulative_value);
        Assert.False(result.UsedFallback);
    }

    [Fact]
    public async Task SelectAsync_NoSampleBeforeStart_UsesEarliestInWindow()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var inWindow = start.AddMinutes(15);
        _repo.Seed(CreateMetric("output", 50, inWindow));

        var result = await _selector.SelectAsync("output", start, start.AddHours(1));

        Assert.NotNull(result.Baseline);
        Assert.Equal(50, result.Baseline!.cumulative_value);
        Assert.True(result.UsedFallback);
    }

    [Fact]
    public async Task SelectAsync_FindsClosingSample()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        _repo.Seed(CreateMetric("output", 100, start));
        _repo.Seed(CreateMetric("output", 145, start.AddHours(1)));

        var result = await _selector.SelectAsync("output", start, start.AddHours(1));

        Assert.NotNull(result.Closing);
        Assert.Equal(145, result.Closing!.cumulative_value);
    }

    [Fact]
    public async Task SelectAsync_NoDataAtAll_ReturnsNullBaselineAndClosing()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = await _selector.SelectAsync("output", start, start.AddHours(1));

        Assert.Null(result.Baseline);
        Assert.Null(result.Closing);
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
