using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public record DocumentIndexResult(long Id, IEnumerable<Hash> Hashes, long LineCount);

public interface IDocumentIndexer
{
    Task<DocumentIndexResult> IndexAsync(IDocument document, CancellationToken ct);
}
