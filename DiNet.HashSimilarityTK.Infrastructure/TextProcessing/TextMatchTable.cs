using DiNet.HashSimilarityTK.Core;
using System.Text.RegularExpressions;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class TextMatchTable<T>(IHashMatchBucket<T> bucket, ITextSignatureGenerator textSignatureGenerator, int signatureSize) : ITextMatchTable<T>
{
    public long Count { get; private set; } = 0;

    public void Add(ReadOnlySpan<char> text, T key)
    {
        var signatureBuffer = (stackalloc Hash[signatureSize]);
        var signatureLength = textSignatureGenerator.Generate(text, signatureBuffer);

        if (signatureLength > 0)
        {
            bucket.Place(signatureBuffer[..signatureLength], key);
        }

        Count++;
    }

    public Match<T> EnumerateAllMatchings()
    {
        return new(bucket.EnumerateAllMatchings());
    }
}
