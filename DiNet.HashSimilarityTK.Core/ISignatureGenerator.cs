namespace DiNet.HashSimilarityTK.Core;

public interface ISignatureGenerator
{
    public int Generate(ReadOnlySpan<Hash> elements, Span<Hash> buffer);
}