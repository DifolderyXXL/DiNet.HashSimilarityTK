namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IConsolePresenter<in TResponse>
{
    void Render(TResponse response);
}

public interface IUntypedConsolePresenter
{
    Type ResponseType { get; }
    void Render(object response);
}

public class UntypedConsolePresenterDecorator<TResponse>(IConsolePresenter<TResponse> presenter)
    : IUntypedConsolePresenter
{
    public Type ResponseType => typeof(TResponse);

    public void Render(object response)
    {
        presenter.Render((TResponse)response);
    }
}