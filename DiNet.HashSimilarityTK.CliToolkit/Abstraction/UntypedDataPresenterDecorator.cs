namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public class UntypedDataPresenterDecorator<TResponse>(IDataPresenter<TResponse> presenter)
    : IUntypedDataPresenter
{
    public Type ResponseType => typeof(TResponse);

    public void Present(object response)
    {
        presenter.Present((TResponse)response);
    }
}