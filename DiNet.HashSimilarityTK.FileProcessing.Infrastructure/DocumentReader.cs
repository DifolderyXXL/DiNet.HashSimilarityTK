using DiNet.HashSimilarityTK.FileProcessing.Core;
using System.Runtime.CompilerServices;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure;

public class DocumentReader : IDocumentReader
{
    public async IAsyncEnumerable<LineData> EnumerateLines(
        IDocument document,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var reader = document.OpenTextReader();
        long originalLineNumber = 0;
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            yield return new LineData(originalLineNumber, line);
            originalLineNumber++;
        }
    }
}
