namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public class UntypedQueryHandlerDecorator<TQuery, TResponse>(IQueryHandler<TQuery, TResponse> handler) : IUntypedQueryHandler
    where TQuery : IQuery<TResponse>
{
    public Type QueryType => typeof(TQuery);

    public async Task<object> HandleAsync(object command, CancellationToken ct)
    {
        return await handler.Handle((TQuery)command, ct);
    }
}
