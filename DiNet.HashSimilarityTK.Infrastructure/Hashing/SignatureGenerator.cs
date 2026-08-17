using DiNet.HashSimilarityTK.Core;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class SignatureGenerator(IMinHashGenerator[] hashers) : ISignatureGenerator
{
    public int Generate(ReadOnlySpan<Hash> elements, Span<Hash> buffer)
    {
        if (elements.IsEmpty) return 0;

        for (int i = 0; i < hashers.Length; i++)
        {
            buffer[i] = hashers[i].GenerateMinHash(elements);
        }

        return hashers.Length;
    }
}


public static class SignatureGeneratorFactory
{
    public static SignatureGenerator Create(uint seed, int hashCount)
    {
        var hashers = Enumerable.Range(0, hashCount)
            .Select(x => new XxHasher((uint)(seed + x)))
            .Select(x => new MinHashGenerator(x))
            .ToArray();

        return new SignatureGenerator(hashers);
    }
}