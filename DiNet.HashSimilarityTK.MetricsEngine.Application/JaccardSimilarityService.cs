using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.MetricsEngine.Core;

namespace DiNet.HashSimilarityTK.MetricsEngine.Application;


public class JaccardSimilarityService : ISimilarityService
{
    public SimilarityTable ComputeSimilarity(DistinctMatch<DocumentFileLine> match, IDocumentLineCountProvider lineCountProvider)
    {
        var groups = match.Result;
        var intersections = new Dictionary<(long, long), HashSet<int>>();

        var groupId = 0;
        foreach (var group in groups)
        {
            var files = group.Select(e => e.DocumentId).Distinct().ToArray();

            for (int i = 0; i < files.Length; ++i)
            {
                for (int j = i + 1; j < files.Length; j++)
                {
                    var key = (Math.Min(files[i], files[j]), Math.Max(files[i], files[j]));
                    if (!intersections.TryGetValue(key, out var hashSet))
                    {
                        hashSet = new();
                        intersections.Add(key, hashSet);
                    }

                    hashSet.Add(groupId);
                }
            }

            ++groupId;
        }

        var scores = new Dictionary<(long, long), double>();
        foreach (var intersection in intersections)
        {
            var intersectionSize = intersection.Value.Count;

            var unionSize =
                lineCountProvider.GetLineCount(intersection.Key.Item1)
                + lineCountProvider.GetLineCount(intersection.Key.Item2)
                - intersectionSize;

            scores.Add(intersection.Key, 1.0 * intersectionSize / unionSize);
        }

        return new(scores);
    }
}
