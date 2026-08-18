using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.MetricsEngine.Core;

namespace DiNet.HashSimilarityTK.MetricsEngine.Application;


public class JaccardSimilarityService : ISimilarityService
{
    public SimilarityTable ComputeSimilarity(DistinctMatch<DocumentFileLine> match, IDocumentStore store)
    {
        var groups = match.Result;


        var candidatePairs = new HashSet<(long, long)>();
        foreach (var group in match.Result)
        {
            var docIds = group.Select(x => x.DocumentId).Distinct().ToList();
            for (int i = 0; i < docIds.Count; i++)
                for (int j = i + 1; j < docIds.Count; j++)
                {
                    long id1 = docIds[i], id2 = docIds[j];
                    if (id1 > id2) (id1, id2) = (id2, id1);
                    candidatePairs.Add((id1, id2));
                }
        }

        var scores = new Dictionary<(long, long), double>();

        foreach (var (docId1, docId2) in candidatePairs)
        {
            var doc1 = store.GetDocument(docId1) as Document;
            var doc2 = store.GetDocument(docId2) as Document;
            if (doc1 == null || doc2 == null) continue;

            var set1 = doc1.StringHashes;
            var set2 = doc2.StringHashes;
            if (set1.Count == 0 || set2.Count == 0) continue;

            int intersection = set1.Intersect(set2).Count();
            int union = set1.Count + set2.Count - intersection;
            if (union == 0) continue;

            scores[(docId1, docId2)] = (double)intersection / union;
        }

        return new(scores);
    }
}
