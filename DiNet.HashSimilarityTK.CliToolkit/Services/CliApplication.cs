namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class CliApplication(
    CommandHandlerResolver resolver,
    ICommandCallRequestBuilder commandCallRequestBuilder,
    IQueryHandlerResolver queryHandlerResolver,
    IPresenterResolver presenterResolver,
    CommandRouteRegistry routeRegistry)
{
    public async Task Route(string[] args, CancellationToken ct)
    {
        if (args.Length == 0) return;

        var route = args[0];
        if (!routeRegistry.TryGetHandlerType(route, out var type) || type is null) return;

        var handler = queryHandlerResolver.Resolve(type);

        var queryArgs = args.Length > 1 ? args.AsSpan(1) : ReadOnlySpan<string>.Empty;
        var query = resolver.Resolve(handler.QueryType, commandCallRequestBuilder.Build(queryArgs));

        var response = await handler.HandleAsync(query, ct);

        if (response is not null)
        {
            var presenter = presenterResolver.TryResolve(response.GetType());
            if (presenter is not null)
            {
                presenter.Present(response);
            }
            else
            {
                Console.WriteLine(response.ToString());
            }
        }
    }
}
