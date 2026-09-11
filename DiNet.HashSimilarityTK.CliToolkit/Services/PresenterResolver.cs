using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class PresenterResolver(IPresenterStore presenterStore, IServiceProvider provider) : IPresenterResolver
{
    public IUntypedDataPresenter? TryResolve(Type responseType)
    {
        var presenterType = presenterStore.GetPresenterTypeFor(responseType);
        if (presenterType is null) return null;

        var rawPresenter = ActivatorUtilities.CreateInstance(provider, presenterType)!;
        var decoratorType = typeof(UntypedDataPresenterDecorator<>).MakeGenericType(responseType);

        return (IUntypedDataPresenter)Activator.CreateInstance(decoratorType, rawPresenter)!;
    }
}
