using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DiNet.HashSimilarityTK.Cli.Services.Telemetry;

public static class Diagnostics
{
    public const string ServiceName = "DiNet.HashSimilarityTK";
    public const string ServiceVersion = "1.0.0";

    public static readonly ActivitySource Source = new(ServiceName, ServiceVersion);
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);

    public static readonly Histogram<double> OperationDuration =
        Meter.CreateHistogram<double>("cli.operation.duration", "ms");
    public static readonly Counter<long> OperationCount =
        Meter.CreateCounter<long>("cli.operation.count");
}