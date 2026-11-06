namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public interface IDocumentStatsStore
{
    public void Set(DocumentIndexResult stats);
}
