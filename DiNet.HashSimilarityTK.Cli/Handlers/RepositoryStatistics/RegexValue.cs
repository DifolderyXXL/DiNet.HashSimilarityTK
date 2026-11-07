using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;

public class RegexValue : IParsable<RegexValue>
{
    public Regex Value { get; }

    private RegexValue(string pattern)
    {
        Value = new Regex(pattern);
    }

    public static RegexValue Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        try
        {
            return new RegexValue(s);
        }
        catch (ArgumentException ex)
        {
            throw new FormatException($"The string '{s}' is not a valid regular expression.", ex);
        }
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out RegexValue result)
    {
        if (s is null)
        {
            result = null;
            return false;
        }

        try
        {
            result = new RegexValue(s);
            return true;
        }
        catch (ArgumentException)
        {
            result = null;
            return false;
        }
    }
}
