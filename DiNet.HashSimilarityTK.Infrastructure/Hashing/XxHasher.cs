using DiNet.HashSimilarityTK.Core;
using System.IO.Hashing;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class XxHasher(uint seed) : IHasher
{
    public Hash Hash(ReadOnlySpan<byte> buffer)
    {
        return new((long)XxHash64.HashToUInt64(buffer, seed));
    }
}
