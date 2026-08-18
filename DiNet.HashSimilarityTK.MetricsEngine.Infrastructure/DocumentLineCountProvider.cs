using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.MetricsEngine.Core;

namespace DiNet.HashSimilarityTK.MetricsEngine.Infrastructure;

public class DocumentLineCountProvider(IDocumentStore documentStore) : IDocumentLineCountProvider
{

    public long GetLineCount(long documentId)
    {
        var doc = documentStore.GetDocument(documentId);
        return doc?.LineCount ?? 0;
    }
}