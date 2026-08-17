namespace DiNet.HashSimilarityTK.Core;

public interface IHashMatchBucket<TKey>
{
    public void Place(ReadOnlySpan<Hash> signature, TKey key);
    public IEnumerable<IEnumerable<TKey>> EnumerateAllMatchings();
}


public interface ITextMatchTable<T>
{
    int Count { get; }
    void Add(ReadOnlySpan<char> text, T key);
    IEnumerable<IEnumerable<T>> EnumerateAllMatchings();
}