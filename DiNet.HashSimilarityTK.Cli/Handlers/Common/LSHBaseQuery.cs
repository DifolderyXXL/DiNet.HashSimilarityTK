namespace DiNet.HashSimilarityTK.Cli.Handlers.Common;

public abstract record LSHBaseQuery : RootBaseQuery
{
    public int ShingleSize { get; set; } = 4;
    public int NumHashes { get; set; } = 128;
    public int ChunkStep { get; set; } = 16;
    public uint Seed { get; set; } = 123456789;
}
