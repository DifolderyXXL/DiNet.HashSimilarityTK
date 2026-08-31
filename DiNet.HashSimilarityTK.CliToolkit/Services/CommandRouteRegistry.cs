namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class CommandRouteRegistry
{
    private readonly Dictionary<string, Type> _routes = new(StringComparer.OrdinalIgnoreCase);

    public CommandRouteRegistry Register<THandler>(string route) => Register(route, typeof(THandler));

    public CommandRouteRegistry Register(string route, Type handlerType)
    {
        _routes[route] = handlerType;
        return this;
    }

    public bool TryGetHandlerType(string route, out Type? handlerType)
        => _routes.TryGetValue(route, out handlerType);
}
