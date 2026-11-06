using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public sealed class ParallelDocumentProcessingService(
    IDocumentStore store,
    IDocumentIndexer indexer,
    IDocumentStatsStore stats)
{
    public async Task ProcessAllDocumentsAsync(CancellationToken ct)
    {
        await Parallel.ForEachAsync(store.GetAllDocuments(), ct, async (d, c) =>
        {
            var result = await indexer.IndexAsync(d, c);
            stats.Set(result);
        });
    }
}
