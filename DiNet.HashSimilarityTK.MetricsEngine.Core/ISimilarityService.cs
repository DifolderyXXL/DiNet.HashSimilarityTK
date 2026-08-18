using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.MetricsEngine.Core;

public interface ISimilarityService
{
    SimilarityTable ComputeSimilarity(DistinctMatch<DocumentFileLine> match, IDocumentLineCountProvider lineCountProvider);
}