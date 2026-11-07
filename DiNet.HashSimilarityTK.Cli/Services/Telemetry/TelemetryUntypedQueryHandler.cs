using DiNet.CliToolkit.Abstraction;

namespace DiNet.HashSimilarityTK.Cli.Services.Telemetry;

internal sealed class TelemetryUntypedQueryHandler(
    IUntypedQueryHandler inner,
    Type handlerType,
    ITelemetry telemetry) : IUntypedQueryHandler
{
    public Type QueryType => inner.QueryType;

    public async Task<object?> HandleAsync(object command, CancellationToken ct)
    {
        var name = handlerType.Name.Replace("Handler", string.Empty);

        return await telemetry.ProcessAsync($"cli.handler.{name}",
            (inner, command),
            static async (state, ct) => await state.inner.HandleAsync(state.command, ct));
    }
}
