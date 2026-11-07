using DiNet.CliToolkit.Abstraction;

namespace DiNet.HashSimilarityTK.Cli.Services.Telemetry;

internal sealed class TelemetryQueryHandlerResolver(IQueryHandlerResolver inner, ITelemetry telemetry) : IQueryHandlerResolver
{
    public IUntypedQueryHandler Resolve(Type handlerType)
        => new TelemetryUntypedQueryHandler(inner.Resolve(handlerType), handlerType, telemetry);
}
