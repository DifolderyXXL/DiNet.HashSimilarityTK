using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure;

public class FilteredDocumentReader(IDocumentReader baseReader, IDocumentLineFilter filter) : IDocumentReader
{
    public IAsyncEnumerable<LineData> EnumerateLines(
        IDocument document,
        CancellationToken ct = default)
    {
        return baseReader.EnumerateLines(document, ct)
            .Where(x => filter.FilterLine(x.Text));
    }
}