using Xunit;
using OpenCode.Api.Aggregation;

namespace OpenCode.Api.Tests;

public class TimeRangeValidatorTests
{
    private readonly TimeRangeValidator _validator = new();

    [Fact]
    public void Validate_ValidRange_ReturnsSuccess()
    {
        var result = _validator.Validate("2026-01-01T10:00:00Z", "2026-01-01T11:00:00Z");

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_MissingStartTime_ReturnsError()
    {
        var result = _validator.Validate(null, "2026-01-01T11:00:00Z");

        Assert.False(result.IsValid);
        Assert.Contains("start_time", result.ErrorMessage);
    }

    [Fact]
    public void Validate_MissingEndTime_ReturnsError()
    {
        var result = _validator.Validate("2026-01-01T10:00:00Z", null);

        Assert.False(result.IsValid);
        Assert.Contains("end_time", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EmptyStartTime_ReturnsError()
    {
        var result = _validator.Validate("", "2026-01-01T11:00:00Z");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidStartTime_ReturnsError()
    {
        var result = _validator.Validate("not-a-date", "2026-01-01T11:00:00Z");

        Assert.False(result.IsValid);
        Assert.Contains("start_time", result.ErrorMessage);
    }

    [Fact]
    public void Validate_InvalidEndTime_ReturnsError()
    {
        var result = _validator.Validate("2026-01-01T10:00:00Z", "not-a-date");

        Assert.False(result.IsValid);
        Assert.Contains("end_time", result.ErrorMessage);
    }

    [Fact]
    public void Validate_StartAfterEnd_ReturnsError()
    {
        var result = _validator.Validate("2026-01-01T11:00:00Z", "2026-01-01T10:00:00Z");

        Assert.False(result.IsValid);
        Assert.Contains("start_time", result.ErrorMessage);
    }

    [Fact]
    public void Validate_IdenticalStartAndEnd_ReturnsSuccess()
    {
        var result = _validator.Validate("2026-01-01T10:00:00Z", "2026-01-01T10:00:00Z");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NonUtcDateTime_InterpretsAsUtc()
    {
        var result = _validator.Validate("2026-01-01T10:00:00", "2026-01-01T11:00:00");

        Assert.True(result.IsValid);
        Assert.Equal(DateTimeKind.Utc, result.StartTime.Kind);
    }
}
