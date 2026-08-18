namespace DiNet.HashSimilarityTK.MetricsEngine.Core;

public class SimilarityTable(Dictionary<(long, long), double> similarity)
{
    public int Count => similarity.Count;

    public double GetSimilarity(long fileA, long fileB)
    {
        if (fileA > fileB) return GetSimilarity(fileB, fileA);

        if (similarity.TryGetValue((fileA, fileB), out var value)) return value;

        return 0;
    }

    public IEnumerable<(long FileA, long FileB)> GetAllPairs()
    {
        foreach (var key in similarity.Keys)
        {
            yield return (key.Item1, key.Item2);
        }
    }

    public IEnumerable<(long FileA, long FileB, double Similarity)> GetAllScores()
    {
        foreach (var kvp in similarity)
        {
            yield return (kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
        }
    }
}
