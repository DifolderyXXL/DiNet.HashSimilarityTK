namespace DiNet.HashSimilarityTK.Core;

public interface IMinHashGenerator
{
    public Hash GenerateMinHash(ReadOnlySpan<Hash> buffer);
}