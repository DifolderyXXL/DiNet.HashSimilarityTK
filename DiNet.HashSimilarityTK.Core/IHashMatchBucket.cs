namespace DiNet.HashSimilarityTK.Core;

public interface IHashMatchBucket<TKey>
{
    public void Place(ReadOnlySpan<Hash> signature, TKey key);
    public IEnumerable<IEnumerable<TKey>> EnumerateAllMatchings();
}
