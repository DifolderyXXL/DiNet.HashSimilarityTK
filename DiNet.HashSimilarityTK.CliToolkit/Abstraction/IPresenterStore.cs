namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IPresenterStore
{
    void Register<TResponse, TPresenter>()
        where TPresenter : IDataPresenter<TResponse>;

    void Register(Type responseType, Type presenterType);

    Type? GetPresenterTypeFor(Type responseType);
}
