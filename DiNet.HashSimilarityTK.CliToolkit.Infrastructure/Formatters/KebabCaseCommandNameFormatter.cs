using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using System.Text.RegularExpressions;


namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure.Formatters;

public class KebabCaseCommandNameFormatter : ICommandNameFormatter
{
    public string Format(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        return Regex.Replace(name, @"(?<!^)(?=[A-Z])", "-").ToLowerInvariant();
    }
}
