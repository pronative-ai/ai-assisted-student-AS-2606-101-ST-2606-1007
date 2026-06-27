using OpenCode.Api.Constants;
using OpenCode.Api.Models;

namespace OpenCode.Api.Aggregation;

public class TokenUsageAggregationService
{
    private readonly BaselineSelector _baselineSelector;

    public TokenUsageAggregationService(BaselineSelector baselineSelector)
    {
        _baselineSelector = baselineSelector;
    }

    public async Task<TokenUsageAggregateResponse> AggregateAsync(
        DateTime startTime, DateTime endTime)
    {
        var response = new TokenUsageAggregateResponse
        {
            start_time = startTime,
            end_time = endTime,
        };

        foreach (var tokenType in OpenCodeSignals.SupportedTokenTypes)
        {
            var result = await _baselineSelector.SelectAsync(tokenType, startTime, endTime);

            if (result.Warning is not null)
                response.warnings.Add(result.Warning);

            if (result.Baseline is null || result.Closing is null)
            {
                SetTokenTotal(response, tokenType, 0);
                continue;
            }

            if (result.Closing.cumulative_value < result.Baseline.cumulative_value)
            {
                response.warnings.Add(
                    $"Counter decrease detected for type '{tokenType}': " +
                    $"{result.Baseline.cumulative_value} -> {result.Closing.cumulative_value}. " +
                    $"Returning 0 for this type.");
                SetTokenTotal(response, tokenType, 0);
                continue;
            }

            var delta = result.Closing.cumulative_value - result.Baseline.cumulative_value;
            SetTokenTotal(response, tokenType, delta);
        }

        return response;
    }

    private static void SetTokenTotal(TokenUsageAggregateResponse response, string tokenType, long value)
    {
        switch (tokenType)
        {
            case "input": response.totals.input = value; break;
            case "output": response.totals.output = value; break;
            case "reasoning": response.totals.reasoning = value; break;
            case "cacheRead": response.totals.cacheRead = value; break;
            case "cacheCreation": response.totals.cacheCreation = value; break;
        }
    }
}
