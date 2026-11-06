using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public sealed class DocumentStatsStore(IDocumentStore store) : IDocumentStatsStore
{
    public void Set(DocumentIndexResult result)
    {
        if (store.GetDocument(result.Id) is not Document concrete)
            return;

        concrete.StringHashes.UnionWith(result.Hashes);
        concrete.SetLineCount(result.LineCount);
    }
}
