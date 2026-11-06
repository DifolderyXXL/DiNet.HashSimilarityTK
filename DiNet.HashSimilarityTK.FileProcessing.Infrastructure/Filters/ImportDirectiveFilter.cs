using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;

public class ImportDirectiveFilter : IDocumentLineFilter
{
    private static readonly string[] ImportPrefixes = { "using ", "import ", "from " };

    public bool FilterLine(string line)
    {
        var trimmed = line.Trim();
        foreach (var prefix in ImportPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                return false;
        }
        return true;
    }
}
