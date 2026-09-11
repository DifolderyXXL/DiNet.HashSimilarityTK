using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using Humanizer;


namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure.Formatters;

public class SnakeCaseNameFormatter : INameFormatter
{
    public string Format(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        return name.Underscore();
    }
}

