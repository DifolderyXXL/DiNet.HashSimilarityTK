using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure;

public readonly record struct LineData(long OriginalLineNumber, string Text);

public interface IDocumentReader
{
    IAsyncEnumerable<LineData> EnumerateLines(IDocument document, CancellationToken ct);
}