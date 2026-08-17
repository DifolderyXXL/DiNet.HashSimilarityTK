using DiNet.HashSimilarityTK.Core;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class MinHashGenerator(IHasher hasher) : IMinHashGenerator
{
    public Hash GenerateMinHash(ReadOnlySpan<Hash> buffer)
    {
        long minhash = long.MaxValue;
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        ref long longRef = ref Unsafe.As<byte, long>(ref MemoryMarshal.GetReference(bytes));
        foreach (var val in buffer)
        {
            longRef = val.value;
            minhash = Math.Min(minhash, hasher.Hash(bytes).value);
        }
        return new(minhash);
    }
}