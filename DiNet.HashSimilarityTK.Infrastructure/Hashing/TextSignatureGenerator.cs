using DiNet.HashSimilarityTK.Core;
using System.Buffers;
using System.Runtime.InteropServices;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class TextSignatureGenerator(IShingleExtractor extractor, ISignatureGenerator generator) : ITextSignatureGenerator
{
    public int Generate(ReadOnlySpan<char> text, Span<Hash> signatureBuffer)
    {
        if (text.IsEmpty) return 0;

        var textBytes = MemoryMarshal.AsBytes(text);

        Hash[] rented = ArrayPool<Hash>.Shared.Rent(text.Length);
        try
        {
            var shingleBuffer = rented.AsSpan(0, text.Length);
            int shingleCount = extractor.Extract(textBytes, shingleBuffer);

            if (shingleCount <= 0) return 0;

            return generator.Generate(shingleBuffer.Slice(0, shingleCount), signatureBuffer);
        }
        finally
        {
            ArrayPool<Hash>.Shared.Return(rented);
        }
    }
}
