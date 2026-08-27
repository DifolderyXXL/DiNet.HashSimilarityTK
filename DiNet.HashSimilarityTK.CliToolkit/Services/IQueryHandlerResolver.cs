using DiNet.HashSimilarityTK.CliToolkit.Abstraction;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public interface IQueryHandlerResolver
{
    public IUntypedQueryHandler Resolve(Type type);
}
