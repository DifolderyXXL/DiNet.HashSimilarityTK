using DiNet.HashSimilarityTK.Core;
using System.Runtime.InteropServices;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class DictionaryHashMatchBucker<T>(int signatureChunkStep, IHasher hasher) : IHashMatchBucket<T>
{
    private Dictionary<Hash, List<T>> _entries = [];

    public IEnumerable<IEnumerable<T>> EnumerateAllMatchings()
    {
        return _entries.Values.Where(bucket => bucket.Count > 1);
    }

    public void Place(ReadOnlySpan<Hash> signature, T key)
    {
        for (int i = 0; i < signature.Length; i += signatureChunkStep)
        {
            int chunkSize = Math.Min(signatureChunkStep, signature.Length - i);
            var bytes = MemoryMarshal.Cast<Hash, byte>(signature.Slice(i, chunkSize));

            var entireHash = hasher.Hash(bytes);

            if (!_entries.TryGetValue(entireHash, out var entries))
            {
                entries = [];
                _entries[entireHash] = entries;
            }
            entries.Add(key);
        }
    }
}
