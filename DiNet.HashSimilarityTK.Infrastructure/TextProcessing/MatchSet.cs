using DiNet.HashSimilarityTK.Core;
using System.Text.RegularExpressions;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class MatchSet<T> : ITextMatchTable<T>
{
    private readonly ITextMatchTable<T> _matchTable;

    public MatchSet(int shingleSize, int numHashes, int chunkStep, uint seed)
    {
        var shingleHasher = new XxHasher(seed);
        var shingle = new ShingleExtractor(shingleSize, shingleHasher);

        var signatureGenerator = SignatureGeneratorFactory.Create(seed, numHashes);
        var textSignature = new TextSignatureGenerator(shingle, signatureGenerator);

        var bucketHasher = new XxHasher(seed + 1000);
        var matchBucket = new DictionaryHashMatchBucker<T>(chunkStep, bucketHasher);

        _matchTable = new TextMatchTable<T>(matchBucket, textSignature, numHashes);
    }

    public long Count => _matchTable.Count;

    public void Add(ReadOnlySpan<char> text, T key)
    {
        _matchTable.Add(text, key);
    }

    public Match<T> EnumerateAllMatchings()
        => _matchTable.EnumerateAllMatchings();
}