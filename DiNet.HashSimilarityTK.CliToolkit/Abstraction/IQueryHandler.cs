using DiNet.HashSimilarityTK.CliToolkit.Abstraction.Results;

namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;


public interface IQuery<TResponse>;
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken ct); 
}


public interface IUntypedQueryHandler
{
    Type QueryType { get; }
    Task<object> HandleAsync(object command, CancellationToken ct);
}

public class UntypedQueryHandlerDecorator<TQuery, TResponse>(IQueryHandler<TQuery, TResponse> handler) : IUntypedQueryHandler
    where TQuery : IQuery<TResponse>
{
    public Type QueryType => typeof(TQuery);

    public async Task<object> HandleAsync(object command, CancellationToken ct)
    {
        return await handler.Handle((TQuery)command, ct);
    }
}
