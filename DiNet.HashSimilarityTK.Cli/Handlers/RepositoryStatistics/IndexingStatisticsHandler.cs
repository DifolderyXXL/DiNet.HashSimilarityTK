using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Core.Attributes;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

namespace DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;

public static class RepositoryIndexHelper
{
    public static IDocumentStore Index(RootBaseQuery baseQuery)
    {
        var fileSystem = new GitIgnoredFileSystem(
            new ExtensionFileSystem(new IOFileSystem(), [".cs", ".ts", ".py"]),
            baseQuery.Ignore);
        var indexer = new FileSystemIndexer(fileSystem);

        var store = indexer.BuildIndex(baseQuery.Root);

        return store;
    }
}

public abstract record RootBaseQuery : IQuery<bool>
{
    [NotFlagged]
    public string? RootPath { get; set; } = null;
    public string? Ignore { get; set; } = null;

    public string Root => RootPath ?? Environment.CurrentDirectory;
}

public record GetIndexingStatisticsQuery(
    bool Verbose = false) : RootBaseQuery;

public class IndexingStatisticsHandler(IDocumentReader reader) : IQueryHandler<GetIndexingStatisticsQuery, bool>
{
    public async Task<bool> Handle(GetIndexingStatisticsQuery query, CancellationToken ct)
    {
        var store = RepositoryIndexHelper.Index(query);

        Console.WriteLine($"Total Documents Processed : {store.DocumentCount:N0}");

        long totalLineCount = 0;
        foreach (var doc in store.GetAllDocuments())
        {
            long lineNumber = 0;
            await foreach (var line in reader.EnumerateLines(doc, ct))
            {
                lineNumber++;
            }

            totalLineCount += lineNumber;

            if (query.Verbose)
            {
                Console.WriteLine($"{doc.FullPath}\t=\t{lineNumber}");
            }
        }

        double averageLinesPerDoc = store.DocumentCount > 0
            ? (double)totalLineCount / store.DocumentCount
            : 0;

        Console.WriteLine($"Total Lines Indexed       : {totalLineCount:N0}");
        Console.WriteLine($"Average Lines per Doc     : {averageLinesPerDoc:F2}");

        return true;
    }
}