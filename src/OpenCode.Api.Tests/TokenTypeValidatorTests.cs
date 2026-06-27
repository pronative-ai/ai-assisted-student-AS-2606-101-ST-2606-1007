using Xunit;
using OpenCode.Api.Ingestion;

namespace OpenCode.Api.Tests;

public class TokenTypeValidatorTests
{
    private readonly TokenTypeValidator _validator = new();

    [Theory]
    [InlineData("input", true)]
    [InlineData("output", true)]
    [InlineData("reasoning", true)]
    [InlineData("cacheRead", true)]
    [InlineData("cacheCreation", true)]
    [InlineData("unsupportedType", false)]
    [InlineData("", false)]
    [InlineData("INPUT", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpectedResult(string? tokenType, bool expected)
    {
        var result = _validator.IsValid(tokenType!);
        Assert.Equal(expected, result);
    }
}
