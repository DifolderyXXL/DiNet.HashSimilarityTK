using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.CliToolkit.Core;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Core.Models;
using DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class CommandHandlerResolver(ICommandCallerStore store, IParameterBinder binder)
{
    public T Resolve<T>(CommandCallRequest request)
    {
        var caller = store.Get<T>() ?? throw new Exception("Handler does not exist");
        var arguments = binder.Bind(caller, request);
        return (T)caller.Constructor.Invoke(arguments);
    }

    public object Resolve(Type type, CommandCallRequest request)
    {
        var caller = store.Get(type) ?? throw new Exception("Handler does not exist");
        var arguments = binder.Bind(caller, request);
        return caller.Constructor.Invoke(arguments);
    }
}
public class CliApplicationBuilder
{
    private readonly PresenterStore _presenterStore = new();
    private readonly Dictionary<string, Type> _typeRouter = new(StringComparer.OrdinalIgnoreCase);

    private ICommandCallRequestBuilder? _commandCallRequestBuilder;
    private ICommandCallerStore? _commandCallerStore;
    private IParameterBinder? _parameterBinder;
    private IQueryHandlerResolver? _queryHandlerResolver;
    private ICommandNameFormatter? _commandNameFormatter;

    public CliApplicationBuilder RegisterHandler<TQuery, TResponse, THandler>(string route)
        where THandler : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        _typeRouter[route] = typeof(THandler);
        return this;
    }


    public CliApplicationBuilder RegisterHandler<THandler>(string route)
    {
        if (!typeof(THandler).IsAssignableTo(typeof(IQueryHandler<,>))) 
            throw new ArgumentException("Invalid handler");

        _typeRouter[route] = typeof(THandler);
        return this;
    }

    public CliApplicationBuilder RegisterHandler(string route, Type handlerType)
    {
        _typeRouter[route] = handlerType;
        return this;
    }

    public CliApplicationBuilder RegisterPresenter<TResponse, TPresenter>()
        where TPresenter : IConsolePresenter<TResponse>
    {
        _presenterStore.Register<TResponse, TPresenter>();
        return this;
    }

    public CliApplicationBuilder RegisterPresenter(Type responseType, Type presenterType)
    {
        _presenterStore.Register(responseType, presenterType);
        return this;
    }

    public CliApplicationBuilder UseRequestBuilder(ICommandCallRequestBuilder requestBuilder)
    {
        _commandCallRequestBuilder = requestBuilder;
        return this;
    }

    public CliApplicationBuilder UseParameterBinder(IParameterBinder binder)
    {
        _parameterBinder = binder;
        return this;
    }

    public CliApplicationBuilder UseCommandCallerStore(ICommandCallerStore store)
    {
        _commandCallerStore = store;
        return this;
    }

    public CliApplicationBuilder UseQueryHandlerResolver(IQueryHandlerResolver resolver)
    {
        _queryHandlerResolver = resolver;
        return this;
    }

    public CliApplicationBuilder UseNameFormatter(ICommandNameFormatter formatter)
    {
        _commandNameFormatter = formatter;
        return this;
    }

    public CliApplication Build()
    {
        var formatter = _commandNameFormatter ?? new DefaultNameFormatter();
        var store = _commandCallerStore ?? new ForcedCommandCallerStore(formatter);
        var binder = _parameterBinder ?? new ParameterBinder(new ParameterDeserializer());
        var requestBuilder = _commandCallRequestBuilder ?? new DefaultCommandCallRequestBuilder();
        var queryHandlerResolver = _queryHandlerResolver ?? new DefaultQueryHandlerResolver();

        var commandHandlerResolver = new CommandHandlerResolver(store, binder);
        var presenterResolver = new PresenterResolver(_presenterStore);

        return new CliApplication(
            commandHandlerResolver,
            requestBuilder,
            queryHandlerResolver,
            presenterResolver,
            _typeRouter
        );
    }
}
public class DefaultQueryHandlerResolver : IQueryHandlerResolver
{
    public IUntypedQueryHandler Resolve(Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            ?? throw new InvalidOperationException($"Type {handlerType.FullName} does not implement IQueryHandler<TQuery, TResponse>");

        var genericArgs = handlerInterface.GetGenericArguments();
        var queryType = genericArgs[0];
        var responseType = genericArgs[1];

        var rawHandler = Activator.CreateInstance(handlerType)
            ?? throw new InvalidOperationException($"Could not instantiate {handlerType.Name}");

        var decoratorType = typeof(UntypedQueryHandlerDecorator<,>).MakeGenericType(queryType, responseType);

        return (IUntypedQueryHandler)Activator.CreateInstance(decoratorType, rawHandler)!;
    }
}
public class DefaultCommandCallRequestBuilder : ICommandCallRequestBuilder
{
    public CommandCallRequest Build(ReadOnlySpan<string> args)
    {
        string? firstUnflagged = null;
        var parameters = new List<ConsoleParameter>();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("-"))
            {
                var name = arg.TrimStart('-');
                string? value = null;

                var equalsIndex = name.IndexOf('=');
                if (equalsIndex != -1)
                {
                    value = name[(equalsIndex + 1)..];
                    name = name[..equalsIndex];
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    value = args[++i];
                }

                parameters.Add(new ConsoleParameter(name, value));
            }
            else if (firstUnflagged is null)
            {
                firstUnflagged = arg;
            }
        }

        return new CommandCallRequest(firstUnflagged, parameters.ToArray());
    }
}
public interface ICommandCallRequestBuilder
{
    public CommandCallRequest Build(ReadOnlySpan<string> args);
}

public interface IQueryHandlerResolver
{
    public IUntypedQueryHandler Resolve(Type type);
}

public interface IPresenterResolver
{
    IUntypedConsolePresenter? TryResolve(Type responseType);
}

public interface IPresenterStore
{
    void Register<TResponse, TPresenter>()
        where TPresenter : IConsolePresenter<TResponse>;

    void Register(Type responseType, Type presenterType);

    Type? GetPresenterTypeFor(Type responseType);
}

public class PresenterResolver(IPresenterStore presenterStore) : IPresenterResolver
{
    public IUntypedConsolePresenter? TryResolve(Type responseType)
    {
        var presenterType = presenterStore.GetPresenterTypeFor(responseType);
        if (presenterType is null) return null;

        var rawPresenter = Activator.CreateInstance(presenterType)!;
        var decoratorType = typeof(UntypedConsolePresenterDecorator<>).MakeGenericType(responseType);

        return (IUntypedConsolePresenter)Activator.CreateInstance(decoratorType, rawPresenter)!;
    }
}


public class CliApplication(
    CommandHandlerResolver resolver,
    ICommandCallRequestBuilder commandCallRequestBuilder,
    IQueryHandlerResolver queryHandlerResolver,
    IPresenterResolver presenterResolver,
    Dictionary<string, Type> typeRouter)
{
   
    public async Task Route(string[] args, CancellationToken ct)
    {
        if (args.Length == 0) return;

        var route = args[0];
        if (!typeRouter.TryGetValue(route, out var type)) return;

        var handler = queryHandlerResolver.Resolve(type);

        var queryArgs = args.Length > 1 ? args.AsSpan(1) : ReadOnlySpan<string>.Empty;
        var query = resolver.Resolve(handler.QueryType, commandCallRequestBuilder.Build(queryArgs));

        var response = await handler.HandleAsync(query, ct);

        if (response is not null)
        {
            var presenter = presenterResolver.TryResolve(response.GetType());
            if (presenter is not null)
            {
                presenter.Render(response);
            }
            else
            {
                Console.WriteLine(response.ToString());
            }
        }
    }
}

public class ForcedCommandCallerStore(ICommandNameFormatter formatter) : ICommandCallerStore
{
    public CommandCaller? Get<T>()
    {
        return CommandCaller.CreateFor(typeof(T), formatter);
    }

    public CommandCaller? Get(Type type)
    {
        return CommandCaller.CreateFor(type, formatter);
    }
}