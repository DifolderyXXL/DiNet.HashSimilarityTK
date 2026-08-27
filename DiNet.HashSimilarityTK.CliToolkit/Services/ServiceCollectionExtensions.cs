using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCliToolkit(this IServiceCollection services, Action<ServiceCollectionExtensionContext>? configure = null)
    {
        var routeRegistry = new CommandRouteRegistry();
        var presenterStore = new PresenterStore();
        configure?.Invoke(new(routeRegistry, presenterStore, services));

        services.AddSingleton(routeRegistry);
        services.AddSingleton<IPresenterStore>(presenterStore);

        services.AddSingleton<ICommandNameFormatter, DefaultNameFormatter>();
        services.AddSingleton<ICommandCallerStore, ForcedCommandCallerStore>();
        services.AddSingleton<IParameterDeserializer, ParameterDeserializer>();
        services.AddSingleton<IParameterBinder, ParameterBinder>();
        services.AddSingleton<ICommandCallRequestBuilder, DefaultCommandCallRequestBuilder>();

        services.AddTransient<CommandHandlerResolver>();
        services.AddTransient<IQueryHandlerResolver, DefaultQueryHandlerResolver>();
        services.AddTransient<IPresenterResolver, PresenterResolver>();

        services.AddTransient<CliApplication>();

        return services;
    }
}

public class ServiceCollectionExtensionContext(CommandRouteRegistry registry, PresenterStore presenter, IServiceCollection collection)
{
    public ServiceCollectionExtensionContext RegisterHandler<T>(string route) where T : class
    {
        registry.Register<T>(route);

        collection.AddTransient<T>();

        return this;
    }

    public ServiceCollectionExtensionContext RegisterPresenter<TResponse, TPresenter>()
            where TPresenter : class, IDataPresenter<TResponse>
    {
        presenter.Register<TResponse, TPresenter>();

        collection.AddTransient<TPresenter>();

        return this;
    }

    public ServiceCollectionExtensionContext RegisterPresenter<TPresenter>()
        where TPresenter : class
    {
        presenter.Register<TPresenter>();

        collection.AddTransient<TPresenter>();

        return this;
    }
}