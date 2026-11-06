using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;
using DiNet.HashSimilarityTK.Infrastructure;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record DocumentMatchContext(
    DistinctMatch<DocumentFileLine> Groups,
    IDocumentStore Store
);

public interface IDocumentMatchService
{
    Task<DocumentMatchContext> ExtractMatchGroupsAsync(
        IDocumentStore store,
        int shingleSize,
        int numHashes,
        int chunkStep,
        uint seed,
        CancellationToken ct = default);
}

public class DocumentMatchService(IDocumentReader reader) : IDocumentMatchService
{
    public async Task<DocumentMatchContext> ExtractMatchGroupsAsync(
        IDocumentStore store,
        int shingleSize,
        int numHashes,
        int chunkStep,
        uint seed,
        CancellationToken ct = default)
    {
        var matchTable = new MatchSet<DocumentFileLine>(
            shingleSize: shingleSize,
            numHashes: numHashes,
            chunkStep: chunkStep,
            seed: seed
        );

        var docIndexer = new DocumentIndexer(new XxHasher(seed), reader, matchTable);
        var stats = new DocumentStatsStore(store);
        var processingService = new ParallelDocumentProcessingService(store, docIndexer, stats);


        await processingService.ProcessAllDocumentsAsync(ct);

        var groups = matchTable.EnumerateAllMatchings().ToDistinctGroups();

        return new DocumentMatchContext(groups, store);
    }
}
