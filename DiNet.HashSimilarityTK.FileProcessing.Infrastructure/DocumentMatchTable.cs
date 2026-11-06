using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure;

public class DocumentMatchTable<TDocumentKey>(
    ITextMatchTable<DocumentFileLine> matchTable,
    Func<IDocument, long, DocumentFileLine> indexer) : IDocumentMatchTable<TDocumentKey>
{
    public async Task Add(IDocument document, CancellationToken ct = default)
    {
        long lineNumber = 0;
        using var reader = document.OpenTextReader();
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            var key = indexer(document, lineNumber);
            matchTable.Add(line, key);
            lineNumber++;
        }
    }

    public IEnumerable<IEnumerable<DocumentFileLine>> GetAllMatches()
        => matchTable.EnumerateAllMatchings().Result;
}
