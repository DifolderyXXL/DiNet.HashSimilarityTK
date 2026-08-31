using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.CliToolkit.Core.Attributes;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.Infrastructure;
using DiNet.HashSimilarityTK.MetricsEngine.Application;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record GetTopFileSimilarityQuery(
    [NotFlagged] string? RootPath = null,
    int ShingleSize = 4,
    int NumHashes = 128,
    int ChunkStep = 16,
    uint Seed = 123456789,
    int Top = 5,
    string? Ignore = null) : IQuery<bool>;

internal class GetTopFileSimilarityHandler : IQueryHandler<GetTopFileSimilarityQuery, bool>
{
    public async Task<bool> Handle(GetTopFileSimilarityQuery query, CancellationToken ct)
{
        var matchTable = new MatchSet<DocumentFileLine>(
            shingleSize: query.ShingleSize,
            numHashes: query.NumHashes,
            chunkStep: query.ChunkStep,
            seed: query.Seed
        );

        var root = query.RootPath ?? Environment.CurrentDirectory;


        var fileSystem = new GitIgnoredFileSystem(query.Ignore);
        var indexer = new DocumentIndexer(fileSystem);

        var store = indexer.BuildIndex(root);


        var processingService = new DocumentProcessingService(
            store,
            new XxHasher(query.Seed),
            matchTable,
            (document, hash, line) => new DocumentFileLine(document.Id, line, hash)
        );

        await processingService.ProcessAllDocumentsAsync(default);

        var groups = matchTable.EnumerateAllMatchings()
            .ToDistinctGroups();

        var jaccard = new JaccardSimilarityService();
        var scores = jaccard.ComputeSimilarity(groups, store);

        var topScores = scores.GetAllScores()
                                        .OrderByDescending(x => x.Similarity)
                                        .Take(query.Top)
                                        .ToList();

        Console.WriteLine($"Top {query.Top} most similar files in: {root}");

        if (!topScores.Any())
        {
            Console.WriteLine("No pairs with similarity found.");
            return true;
        }

        foreach (var score in topScores)
        {
            var fileA = store.GetDocument(score.FileA)?.FullPath ?? "Unknown";
            var fileB = store.GetDocument(score.FileB)?.FullPath ?? "Unknown";
            Console.WriteLine($"{fileA} <-> {fileB} : {score.Similarity * 100:F2}%");
        }

        return true;
    }
}