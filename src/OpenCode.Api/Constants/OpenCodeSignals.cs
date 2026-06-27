namespace OpenCode.Api.Constants;

public static class OpenCodeSignals
{
    public const string MetricNameTokenUsage = "opencode.token.usage";

    public static readonly string[] SupportedTokenTypes =
        ["input", "output", "reasoning", "cacheRead", "cacheCreation"];

    public static readonly HashSet<string> SupportedTokenTypeSet =
        new(SupportedTokenTypes, StringComparer.Ordinal);

    public const string LogEventApiRequest = "api_request";
    public const string LogEventApiError = "api_error";

    public static readonly string[] SupportedLogEvents =
        [LogEventApiRequest, LogEventApiError];

    public static readonly HashSet<string> SupportedLogEventSet =
        new(SupportedLogEvents, StringComparer.Ordinal);

    public const string UnitTokens = "tokens";

    public const string RecordTypeTokenUsageMetric = "token_usage_metric";
    public const string RecordTypeOpenCodeLog = "opencode_log";
}
