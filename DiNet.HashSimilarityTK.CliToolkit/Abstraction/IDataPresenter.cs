namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IDataPresenter<in TResponse>
{
    void Present(TResponse response);
}

public interface IUntypedDataPresenter
{
    Type ResponseType { get; }
    void Present(object response);
}

public class UntypedDataPresenterDecorator<TResponse>(IDataPresenter<TResponse> presenter)
    : IUntypedDataPresenter
{
    public Type ResponseType => typeof(TResponse);

    public void Present(object response)
    {
        presenter.Present((TResponse)response);
    }
}