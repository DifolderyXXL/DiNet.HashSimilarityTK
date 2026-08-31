using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using Humanizer;


namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure.Formatters;

public class KebabCaseNameFormatter : INameFormatter
{
    public string Format(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        return name.Kebaberize();
    }
}

