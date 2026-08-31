using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Infrastructure;
using DiNet.HashSimilarityTK.CliToolkit.Infrastructure.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCliToolkit(
        this IServiceCollection services,
        Action<CliToolkitBuilder> configure)
    {
        var builder = new CliToolkitBuilder(services);
        configure(builder);
        builder.Build();
        return services;
    }
}

public class CliToolkitBuilder
{
    private readonly IServiceCollection _services;
    private readonly CommandRouteRegistry _routeRegistry;
    private readonly PresenterStore _presenterStore;
    private Type? _formatterType;

    public CliToolkitBuilder(IServiceCollection services)
    {
        _services = services;
        _routeRegistry = new CommandRouteRegistry();
        _presenterStore = new PresenterStore();
    }

    public HandlerRegistry Handlers => new(_routeRegistry, _services);

    public PresenterRegistry Presenters => new(_presenterStore, _services);

    public CliToolkitBuilder UseKebabCaseFormatter()
    {
        _formatterType = typeof(KebabCaseNameFormatter);
        return this;
    }

    public CliToolkitBuilder UseCamelCaseFormatter()
    {
        _formatterType = typeof(CamelCaseNameFormatter);
        return this;
    }

    public CliToolkitBuilder UseCaseFormatter<T>() where T : class, INameFormatter
    {
        _formatterType = typeof(T);
        return this;
    }

    public CliToolkitBuilder UseFormatter<TFormatter>()
        where TFormatter : class, INameFormatter
    {
        _formatterType = typeof(TFormatter);
        return this;
    }

    internal void Build()
    {
        _services.AddSingleton(_routeRegistry);
        _services.AddSingleton<IPresenterStore>(_presenterStore);

        var formatterType = _formatterType ?? typeof(DefaultNameFormatter);
        _services.AddSingleton(typeof(INameFormatter), formatterType);

        _services.AddSingleton<ICommandCallerStore, ReflectionCommandCallerStore>();
        _services.AddSingleton<IParameterDeserializer, ParameterDeserializer>();
        _services.AddSingleton<IParameterBinder, ParameterBinder>();
        _services.AddSingleton<ICommandCallRequestBuilder, StrictCommandCallRequestBuilder>();

        _services.AddTransient<ICommandHandlerFactory, CommandHandlerFactory>();
        _services.AddTransient<IQueryHandlerResolver, DefaultQueryHandlerResolver>();
        _services.AddTransient<IPresenterResolver, PresenterResolver>();

        _services.AddTransient<CliApplication>();
    }
}

public class HandlerRegistry
{
    private readonly CommandRouteRegistry _registry;
    private readonly IServiceCollection _services;

    public HandlerRegistry(CommandRouteRegistry registry, IServiceCollection services)
    {
        _registry = registry;
        _services = services;
    }

    public HandlerRegistry Register<THandler>(string route) where THandler : class
    {
        _registry.Register<THandler>(route);
        _services.AddTransient<THandler>();
        return this;
    }
}

public class PresenterRegistry
{
    private readonly PresenterStore _store;
    private readonly IServiceCollection _services;

    public PresenterRegistry(PresenterStore store, IServiceCollection services)
    {
        _store = store;
        _services = services;
    }

    public PresenterRegistry Register<TResponse, TPresenter>()
        where TPresenter : class, IDataPresenter<TResponse>
    {
        _store.Register<TResponse, TPresenter>();
        _services.AddTransient<TPresenter>();
        return this;
    }
}