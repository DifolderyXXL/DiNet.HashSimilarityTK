using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DiNet.HashSimilarityTK.Cli.Services.Telemetry;

public interface ITelemetry
{
    Task<T> ProcessAsync<T>(string name,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default);

    Task ProcessAsync(string name,
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default);


    T Process<T>(string name,
        Func<T> operation);

    void Process(string name,
        Action operation);
    Task<TResult> ProcessAsync<TState, TResult>(string name, TState state, Func<TState, CancellationToken, Task<TResult>> operation, CancellationToken ct = default);
    TResult Process<TState, TResult>(string name, TState state, Func<TState, TResult> operation);
}

internal sealed class Telemetry(ILogger<Telemetry> log, ActivitySource source) : ITelemetry
{
    private bool IsDebug => log.IsEnabled(LogLevel.Debug);
    private bool HasTracing => source.HasListeners();

    public async Task<TResult> ProcessAsync<TState, TResult>(
        string name,
        TState state,
        Func<TState, CancellationToken, Task<TResult>> operation,
        CancellationToken ct = default)
    {
        if (!IsDebug && !HasTracing)
        {
            return await operation(state, ct).ConfigureAwait(false);
        }


        using var activity = source.StartActivity(name);
        activity?.SetTag("cli.process", name);

        var ok = false;
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            var result = await operation(state, ct);
            ok = true;
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
        finally
        {
            Report(startTimestamp, name, ok);
        }
    }

    public Task<T> ProcessAsync<T>(
        string name,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default) => ProcessAsync(
            name,
            operation,
            static (op, token) => op(token),
            ct);

    public Task ProcessAsync(
        string name,
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default) => ProcessAsync(
            name,
            operation,
            static async (op, token) =>
            {
                await op(token);
                return true;
            },
            ct);

    public TResult Process<TState, TResult>(string name, TState state, Func<TState, TResult> operation)
    {
        if (!IsDebug && !HasTracing)
        {
            return operation(state);
        }

        using var activity = source.StartActivity(name);
        activity?.SetTag("cli.process", name);

        var ok = false;
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            var result = operation(state);
            ok = true;
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
        finally
        {
            Report(startTimestamp, name, ok);
        }
    }

    public T Process<T>(string name, Func<T> operation) => Process(
            name,
            operation,
            static op => op());

    public void Process(string name, Action operation) => Process(
            name,
            operation,
            static op => { op(); return 0; });

    private void Report(long startTimestamp, string name, bool ok)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
        var tags = new TagList { { "operation", name }, { "ok", ok } };

        Diagnostics.OperationDuration.Record(elapsedMs, in tags);
        Diagnostics.OperationCount.Add(1, in tags);
    }
}
