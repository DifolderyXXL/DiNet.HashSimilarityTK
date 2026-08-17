namespace DiNet.HashSimilarityTK.Core;

public interface IHasher
{
    public Hash Hash(ReadOnlySpan<byte> buffer);
}
