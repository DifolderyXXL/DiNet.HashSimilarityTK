using DiNet.HashSimilarityTK.Core;

namespace DiNet.HashSimilarityTK.Infrastructure;

public class ShingleExtractor(int bagLength, IHasher hasher) : IShingleExtractor
{
    public int Extract(ReadOnlySpan<byte> line, Span<Hash> shingles)
    {
        if (line.Length < bagLength) return 0;

        int count = line.Length - bagLength + 1;
        int maxItems = Math.Min(count, shingles.Length);

        for (int i = 0; i < maxItems; i++)
        {
            shingles[i] = hasher.Hash(line.Slice(i, bagLength));
        }

        return maxItems;
    }
}
