using DiNet.HashSimilarityTK.CliToolkit.Abstraction.Results;

namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;


public interface IQuery<TResponse>;
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct); 
}
