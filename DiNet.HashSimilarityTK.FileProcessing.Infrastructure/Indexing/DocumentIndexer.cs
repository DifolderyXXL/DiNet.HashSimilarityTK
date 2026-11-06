using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using System.Text;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public sealed class DocumentIndexer(
    IHasher hasher,
    IDocumentReader reader,
    ITextMatchTable<DocumentFileLine> matchTable) : IDocumentIndexer
{
    public async Task<DocumentIndexResult> IndexAsync(IDocument document, CancellationToken ct)
    {
        var hashes = new List<Hash>();
        long lastLine = -1;

        await foreach (var line in reader.EnumerateLines(document, ct))
        {
            var hash = hasher.Hash(Encoding.UTF8.GetBytes(line.Text));
            matchTable.Add(line.Text, new DocumentFileLine(document.Id, line.OriginalLineNumber, hash));
            hashes.Add(hash);
            lastLine = line.OriginalLineNumber;
        }

        return new DocumentIndexResult(document.Id, hashes, lastLine + 1);
    }
}
