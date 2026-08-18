namespace DiNet.HashSimilarityTK.MetricsEngine.Core;

public interface IDocumentLineCountProvider
{
    long GetLineCount(long documentId);
}
