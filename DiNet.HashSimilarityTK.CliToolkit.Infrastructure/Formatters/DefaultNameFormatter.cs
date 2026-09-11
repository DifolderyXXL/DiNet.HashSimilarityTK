using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure.Formatters;

public class DefaultNameFormatter : INameFormatter
{
    public string Format(string rawName)
        => rawName;
}
