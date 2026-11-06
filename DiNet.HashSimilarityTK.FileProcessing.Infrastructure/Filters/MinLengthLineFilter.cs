using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;

public class MinLengthLineFilter : IDocumentLineFilter
{
    private readonly int _minLength;

    public MinLengthLineFilter(int minLength = 3)
    {
        _minLength = minLength;
    }

    public bool FilterLine(string line)
        => line.Trim().Length >= _minLength;
}
