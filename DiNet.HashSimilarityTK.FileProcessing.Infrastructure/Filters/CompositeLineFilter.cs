using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;

public class CompositeLineFilter : IDocumentLineFilter
{
    private readonly List<IDocumentLineFilter> _filters;

    public CompositeLineFilter(IEnumerable<IDocumentLineFilter> filters)
    {
        _filters = filters.ToList();
    }

    public bool FilterLine(string line)
    {
        for (int i = 0; i < _filters.Count; i++)
        {
            if (!_filters[i].FilterLine(line))
                return false; // Short-circuit evaluation
        }
        return true;
    }
}
