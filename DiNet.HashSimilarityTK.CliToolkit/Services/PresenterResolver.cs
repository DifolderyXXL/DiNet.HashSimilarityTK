using DiNet.HashSimilarityTK.CliToolkit.Abstraction;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class PresenterResolver(IPresenterStore presenterStore) : IPresenterResolver
{
    public IUntypedDataPresenter? TryResolve(Type responseType)
    {
        var presenterType = presenterStore.GetPresenterTypeFor(responseType);
        if (presenterType is null) return null;

        var rawPresenter = Activator.CreateInstance(presenterType)!;
        var decoratorType = typeof(UntypedDataPresenterDecorator<>).MakeGenericType(responseType);

        return (IUntypedDataPresenter)Activator.CreateInstance(decoratorType, rawPresenter)!;
    }
}
