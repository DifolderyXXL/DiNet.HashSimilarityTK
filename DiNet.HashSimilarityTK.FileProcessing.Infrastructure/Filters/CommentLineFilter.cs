using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;

public class CommentLineFilter : IDocumentLineFilter
{
    private static readonly string[] CommentPrefixes = { "//", "/*", "*", "<!--" };

    public bool FilterLine(string line)
    {
        var trimmed = line.Trim();
        foreach (var prefix in CommentPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                return false;
        }
        return true;
    }
}
