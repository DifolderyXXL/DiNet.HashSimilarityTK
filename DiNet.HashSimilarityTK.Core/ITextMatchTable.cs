using System.Text.RegularExpressions;

namespace DiNet.HashSimilarityTK.Core;

public interface ITextMatchTable<T>
{
    long Count { get; }
    void Add(ReadOnlySpan<char> text, T key);
    Match<T> EnumerateAllMatchings();
}

public record class Match<T>(IEnumerable<IEnumerable<T>> Result)
{
    public DistinctMatch<T> ToDistinctGroups()
    {
        return new(Result.Distinct(new GroupComparer<T>()));
    }
}
public record class DistinctMatch<T>(IEnumerable<IEnumerable<T>> Result);

public class GroupComparer<T> : IEqualityComparer<IEnumerable<T>>
{
    public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return new HashSet<T>(x).SetEquals(y);
    }

    public int GetHashCode(IEnumerable<T> obj)
    {
        int hash = 0;
        foreach (var item in obj)
            hash ^= item.GetHashCode();
        return hash;
    }
}