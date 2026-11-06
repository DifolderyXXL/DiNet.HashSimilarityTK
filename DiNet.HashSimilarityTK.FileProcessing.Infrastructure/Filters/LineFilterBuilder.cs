using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;

public class LineFilterBuilder
{
    private readonly List<IDocumentLineFilter> _filters = new();

    public LineFilterBuilder IgnoreWhitespace()
    {
        _filters.Add(new NonEmptyLineFilter());
        return this;
    }

    public LineFilterBuilder MinLength(int minLength)
    {
        _filters.Add(new MinLengthLineFilter(minLength));
        return this;
    }

    public LineFilterBuilder IgnoreStructuralSymbols()
    {
        _filters.Add(new StructuralSymbolFilter());
        return this;
    }

    public LineFilterBuilder IgnoreComments()
    {
        _filters.Add(new CommentLineFilter());
        return this;
    }

    public LineFilterBuilder IgnoreImports()
    {
        _filters.Add(new ImportDirectiveFilter());
        return this;
    }

    public LineFilterBuilder AddCustom(IDocumentLineFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    public LineFilterBuilder AddCustom(Func<string, bool> predicate)
    {
        _filters.Add(new AnonymousDelegateFilter(predicate));
        return this;
    }

    public IDocumentLineFilter Build()
    {
        if (_filters.Count == 0)
            return new NonEmptyLineFilter();

        if (_filters.Count == 1)
            return _filters[0];

        return new CompositeLineFilter(_filters);
    }

    private class AnonymousDelegateFilter : IDocumentLineFilter
    {
        private readonly Func<string, bool> _predicate;
        public AnonymousDelegateFilter(Func<string, bool> predicate) => _predicate = predicate;
        public bool FilterLine(string line) => _predicate(line);
    }
}