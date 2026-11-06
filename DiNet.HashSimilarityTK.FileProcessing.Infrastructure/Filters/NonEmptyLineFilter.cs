using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;

public class NonEmptyLineFilter : IDocumentLineFilter
{
    public bool FilterLine(string line)
        => !string.IsNullOrWhiteSpace(line);
}
