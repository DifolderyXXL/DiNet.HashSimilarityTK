namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IQueryHandlerResolver
{
    public IUntypedQueryHandler Resolve(Type type);
}
