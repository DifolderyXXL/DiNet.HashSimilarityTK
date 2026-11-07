using DiNet.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.Cli.Handlers.Common;
using DiNet.HashSimilarityTK.Cli.Services.Telemetry;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;
using System.Text.RegularExpressions;

namespace DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;


public static class RepositoryIndexHelper
{
    public static IDocumentStore Index(RootBaseQuery baseQuery)
    {
        var fileSystem = new GitIgnoredFileSystem(
            new RegexExtensionFileSystem(
                new IOFileSystem(),
                baseQuery.OnlyExtensions?.Value ?? new Regex(".*")),
            baseQuery.Ignore);
        var indexer = new FileSystemIndexer(fileSystem);

        var store = indexer.BuildIndex(baseQuery.Root);

        return store;
    }
}

public record GetIndexingStatisticsQuery(
    bool Verbose = false) : RootBaseQuery;

public class IndexingStatisticsHandler(IDocumentReader reader, ITelemetry telemetry) : IQueryHandler<GetIndexingStatisticsQuery, bool>
{
    public async Task<bool> Handle(GetIndexingStatisticsQuery query, CancellationToken ct)
    {
        var store = telemetry.Process("Index", () => RepositoryIndexHelper.Index(query));

        
        var uniqueExtensions = new HashSet<string>();
        long totalLineCount = 0;
        foreach (var doc in store.GetAllDocuments())
        {
            uniqueExtensions.Add(Path.GetExtension(doc.FullPath));
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

        Console.WriteLine($"Total Documents Processed : {store.DocumentCount:N0}");

        Console.WriteLine($"Total Lines Indexed       : {totalLineCount:N0}");
        Console.WriteLine($"Average Lines per Doc     : {averageLinesPerDoc:F2}");
        Console.WriteLine($"Unique file extensions    : {string.Join(", ", uniqueExtensions)}");

        return true;
    }
}