using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure;

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

public class NonEmptyLineFilter : IDocumentLineFilter
{
    public bool FilterLine(string line)
        => !string.IsNullOrWhiteSpace(line);
}

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

public class StructuralSymbolFilter : IDocumentLineFilter
{
    private static readonly HashSet<string> StructuralSymbols = new(StringComparer.Ordinal)
    {
        "{", "}", "(", ")", ";", "};", "},", "[]", "{}"
    };

    public bool FilterLine(string line)
        => !StructuralSymbols.Contains(line.Trim());
}


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