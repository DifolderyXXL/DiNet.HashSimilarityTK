using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Core.Attributes;
using DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;

namespace DiNet.HashSimilarityTK.Cli.Handlers.Common;

public abstract record RootBaseQuery : IQuery<bool>
{
    [NotFlagged]
    public string? RootPath { get; set; } = null;
    public string? Ignore { get; set; } = null;
    [Leaf]
    public RegexValue? OnlyExtensions { get; set; } = null;

    public string Root => RootPath ?? Environment.CurrentDirectory;
}
