using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public sealed class DocumentProcessingService(
    IDocumentStore store,
    IDocumentIndexer indexer,
    IDocumentStatsStore stats)
{
    public async Task ProcessAllDocumentsAsync(CancellationToken ct)
    {
        foreach (var doc in store.GetAllDocuments())
        {
            var result = await indexer.IndexAsync(doc, ct);
            stats.Set(result);
        }
    }
}
