using DiNet.HashSimilarityTK.Core;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class TextMatchTable<T>(IHashMatchBucket<T> bucket, ITextSignatureGenerator textSignatureGenerator) : ITextMatchTable<T>
{
    public int Count { get; private set; } = 0;

    public void Add(ReadOnlySpan<char> text, T key)
    {
        var signatureBuffer = (stackalloc Hash[256]);
        var signatureLength = textSignatureGenerator.Generate(text, signatureBuffer);

        if (signatureLength > 0)
        {
            bucket.Place(signatureBuffer[..signatureLength], key);
        }

        Count++;
    }

    public IEnumerable<IEnumerable<T>> EnumerateAllMatchings()
    {
        return bucket.EnumerateAllMatchings();
    }
}

public class MatchSet<T> : ITextMatchTable<T>
{
    private readonly ITextMatchTable<T> _matchTable;

    public MatchSet(int shingleSize, int numHashes, int chunkStep, uint seed)
    {
        var shingleHasher = new XxHasher(seed);
        var shingle = new ShingleExtractor(4, shingleHasher);

        var signatureGenerator = SignatureGeneratorFactory.Create(seed, numHashes);
        var textSignature = new TextSignatureGenerator(shingle, signatureGenerator);

        var bucketHasher = new XxHasher(seed + 1000);
        var matchBucket = new DictionaryHashMatchBucker<T>(chunkStep, bucketHasher);

        _matchTable = new TextMatchTable<T>(matchBucket, textSignature);
    }

    public int Count => _matchTable.Count;

    public void Add(ReadOnlySpan<char> text, T key)
    {
        _matchTable.Add(text, key);
    }

    public IEnumerable<IEnumerable<T>> EnumerateAllMatchings()
        => _matchTable.EnumerateAllMatchings();
}