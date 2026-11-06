using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.MetricsEngine.Application;

public class SequenceResult
{
    public long DocA { get; set; }
    public long DocB { get; set; }
    public long StartLineA { get; set; }
    public long StartLineB { get; set; }
    public long Length { get; set; }
}

public class StraightSimilarityBlockService
{
    private readonly struct LinePair
    {
        public readonly long DocA;
        public readonly long DocB;
        public readonly long Offset;
        public readonly long LineA;

        public LinePair(long docA, long docB, long offset, long lineA)
        {
            DocA = docA;
            DocB = docB;
            Offset = offset;
            LineA = lineA;
        }
    }

    public SequenceResult? ComputeSimilarity(DistinctMatch<DocumentFileLine> match, long maxBucketSizeLimit, bool compareDifferentDocuments)
    {
        var groups = match.Result;

        var pairs = CalculateIntersections(match, maxBucketSizeLimit, compareDifferentDocuments);


        pairs.Sort((x, y) =>
        {
            int cmp = x.DocA.CompareTo(y.DocA);
            if (cmp != 0) return cmp;
            cmp = x.DocB.CompareTo(y.DocB);
            if (cmp != 0) return cmp;
            cmp = x.Offset.CompareTo(y.Offset);
            if (cmp != 0) return cmp;
            return x.LineA.CompareTo(y.LineA);
        });


        long maxLen = 0;
        long bestStartLineA = -1;
        long bestDocA = 0, bestDocB = 0, bestOffset = 0;

        long currentLen = 1;
        long currentStartLineA = pairs[0].LineA;

        for (int i = 1; i < pairs.Count; i++)
        {
            var prev = pairs[i - 1];
            var curr = pairs[i];

            bool sameDiagonal = curr.DocA == prev.DocA &&
                                 curr.DocB == prev.DocB &&
                                 curr.Offset == prev.Offset;

            if (sameDiagonal)
            {
                if (curr.LineA == prev.LineA + 1)
                {
                    currentLen++;
                }
                else if (curr.LineA > prev.LineA + 1)
                {
                    if (currentLen > maxLen)
                    {
                        maxLen = currentLen;
                        bestStartLineA = currentStartLineA;
                        bestDocA = prev.DocA;
                        bestDocB = prev.DocB;
                        bestOffset = prev.Offset;
                    }
                    currentLen = 1;
                    currentStartLineA = curr.LineA;
                }
            }
            else
            {
                if (currentLen > maxLen)
                {
                    maxLen = currentLen;
                    bestStartLineA = currentStartLineA;
                    bestDocA = prev.DocA;
                    bestDocB = prev.DocB;
                    bestOffset = prev.Offset;
                }

                currentLen = 1;
                currentStartLineA = curr.LineA;
            }
        }
        if (currentLen > maxLen)
        {
            maxLen = currentLen;
            bestStartLineA = currentStartLineA;
            var last = pairs[^1];
            bestDocA = last.DocA;
            bestDocB = last.DocB;
            bestOffset = last.Offset;
        }

        if (maxLen == 0) return null;

        return new SequenceResult
        {
            DocA = bestDocA,
            DocB = bestDocB,
            StartLineA = bestStartLineA,
            StartLineB = bestStartLineA - bestOffset,
            Length = maxLen
        };
    }

    private List<LinePair> CalculateIntersections(DistinctMatch<DocumentFileLine> match, long maxBucketSizeLimit, 
        bool compareDifferentDocuments)
    {
        var groups = match.Result;

        var pairs = new List<LinePair>();

        foreach (var group in groups)
        {
            var arr = group as IReadOnlyList<DocumentFileLine> ?? group.ToList(); ;

            if (arr.Count > maxBucketSizeLimit || arr.Count < 2) continue;

            for (int i = 0; i < arr.Count; i++)
            {
                for (int j = i + 1; j < arr.Count; j++)
                {
                    var a = arr[i];
                    var b = arr[j];

                    if (compareDifferentDocuments && a.DocumentId == b.DocumentId)
                        continue;

                    if (a.DocumentId > b.DocumentId || (a.DocumentId == b.DocumentId && a.LineIndex > b.LineIndex))
                    {
                        (a, b) = (b, a);
                    }

                    long offset = a.LineIndex - b.LineIndex;
                    pairs.Add(new LinePair(a.DocumentId, b.DocumentId, offset, a.LineIndex));
                }
            }
        }

        return pairs;
    }
}