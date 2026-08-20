using DiNet.HashSimilarityTK.Cli.Abstraction;
using DiNet.HashSimilarityTK.Cli.Abstraction.Results;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record GetTopFileSimilarityQuery() : IQuery<GetTopFileSimilarityResponse>;
public record GetTopFileSimilarityResponse();
internal class GetTopFileSimilarityHandler : IQueryHandler<GetTopFileSimilarityQuery, GetTopFileSimilarityResponse>
{
    public Task<Result<GetTopFileSimilarityResponse>> Handle(GetTopFileSimilarityQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
