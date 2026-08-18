namespace DiNet.HashSimilarityTK.MetricsEngine.Core;

public class SimilarityTable(Dictionary<(long, long), double> similarity)
{
    public double GetSimilarity(long fileA, long fileB)
    {
        if (fileA > fileB) return GetSimilarity(fileB, fileA);

        if (similarity.TryGetValue((fileA, fileB), out var value)) return value;

        return 0;
    }
}
