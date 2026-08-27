using DiNet.HashSimilarityTK.CliToolkit.Abstraction;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public interface IPresenterStore
{
    void Register<TResponse, TPresenter>()
        where TPresenter : IDataPresenter<TResponse>;

    void Register(Type responseType, Type presenterType);

    Type? GetPresenterTypeFor(Type responseType);
}
