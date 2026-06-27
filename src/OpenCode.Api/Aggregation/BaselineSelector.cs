using OpenCode.Api.Models;
using OpenCode.Api.Persistence;

namespace OpenCode.Api.Aggregation;

public class BaselineSelectionResult
{
    public TokenUsageMetricRecord? Baseline { get; set; }
    public TokenUsageMetricRecord? Closing { get; set; }
    public bool UsedFallback { get; set; }
    public string? Warning { get; set; }
}

public class BaselineSelector
{
    private readonly ICosmosTelemetryRepository _repository;
    private readonly string _studentKey;

    public BaselineSelector(ICosmosTelemetryRepository repository, string studentKey)
    {
        _repository = repository;
        _studentKey = studentKey;
    }

    public async Task<BaselineSelectionResult> SelectAsync(
        string tokenType, DateTime startTime, DateTime endTime)
    {
        var result = new BaselineSelectionResult();

        var baseline = await _repository.GetLatestSampleAtOrBeforeAsync(
            _studentKey, "opencode.token.usage", tokenType, startTime);

        if (baseline is not null)
        {
            result.Baseline = baseline;
        }
        else
        {
            var fallback = await _repository.GetEarliestSampleInWindowAsync(
                _studentKey, "opencode.token.usage", tokenType, startTime, endTime);

            if (fallback is not null)
            {
                result.Baseline = fallback;
                result.UsedFallback = true;
                result.Warning = $"No baseline sample at or before {startTime:O} for type '{tokenType}'. " +
                                 $"Using earliest available sample at {fallback.sample_timestamp_utc:O} as baseline.";
            }
        }

        var closing = await _repository.GetLatestSampleAtOrBeforeAsync(
            _studentKey, "opencode.token.usage", tokenType, endTime);

        if (closing is not null)
        {
            result.Closing = closing;
        }

        return result;
    }
}
