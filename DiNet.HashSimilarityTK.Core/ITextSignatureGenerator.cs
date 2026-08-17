namespace DiNet.HashSimilarityTK.Core;

public interface ITextSignatureGenerator
{
    public int Generate(ReadOnlySpan<char> text, Span<Hash> signatureBuffer);
}