using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

public class DefaultNameFormatter : ICommandNameFormatter
{
    public string Format(string rawName)
        => rawName;
}
