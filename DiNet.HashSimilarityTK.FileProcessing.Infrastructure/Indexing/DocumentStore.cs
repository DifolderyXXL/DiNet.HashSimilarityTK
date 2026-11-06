using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public class DocumentStore : IDocumentStore
{
    private readonly Dictionary<long, IDocument> _documents = new();

    public long DocumentCount => _documents.Count;

    public void AddDocument(IDocument doc) => _documents[doc.Id] = doc;

    public IDocument? GetDocument(long id) => _documents.GetValueOrDefault(id);

    public IEnumerable<IDocument> GetAllDocuments() => _documents.Values;
}
