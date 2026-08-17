namespace DiNet.HashSimilarityTK.Core;

public interface IShingleExtractor
{
    public int Extract(ReadOnlySpan<byte> line, Span<Hash> shingles);
}