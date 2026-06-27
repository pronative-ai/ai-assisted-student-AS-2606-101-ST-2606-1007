using OpenCode.Api.Constants;

namespace OpenCode.Api.Ingestion;

public class TokenTypeValidator
{
    public bool IsValid(string tokenType)
    {
        return OpenCodeSignals.SupportedTokenTypeSet.Contains(tokenType);
    }
}
