using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;

public class StructuralSymbolFilter : IDocumentLineFilter
{
    private static readonly HashSet<string> StructuralSymbols = new(StringComparer.Ordinal)
    {
        "{", "}", "(", ")", ";", "};", "},", "[]", "{}"
    };

    public bool FilterLine(string line)
        => !StructuralSymbols.Contains(line.Trim());
}
