using DiNet.HashSimilarityTK.CliToolkit.Core;
using DiNet.HashSimilarityTK.CliToolkit.Core.Attributes;
using DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Core.Models;
using DiNet.HashSimilarityTK.CliToolkit.Infrastructure;
using DiNet.HashSimilarityTK.CliToolkit.Infrastructure.Formatters;

namespace DiNet.HashSimilarityTK.CliToolkit.Tests;

public class PositionalHandler
{
    public int Id { get; }
    public string Name { get; }
    public bool Verbose { get; }
    public double? Ratio { get; }

    public PositionalHandler([NotFlagged] int id, string name, bool verbose = false, double? ratio = 1.5)
    {
        Id = id;
        Name = name;
        Verbose = verbose;
        Ratio = ratio;
    }
}


public class SampleHandler
{
    public int Id { get; }
    public string Name { get; }
    public bool Verbose { get; }
    public double? Ratio { get; }

    public SampleHandler(int id, string name, bool verbose = false, double? ratio = 1.5)
    {
        Id = id;
        Name = name;
        Verbose = verbose;
        Ratio = ratio;
    }
}



public class ParameterBinderTests
{
    private readonly ParameterBinder _binder;
    private readonly INameFormatter _formatter = new DefaultNameFormatter();

    public ParameterBinderTests()
    {
        var deserializer = new ParameterDeserializer();
        _binder = new ParameterBinder(deserializer);
    }

    [Fact]
    public void Bind_PositionalHandler_AllParameters_ReturnsCorrectArray()
    {
        var caller = CommandCaller.CreateFor<PositionalHandler>(_formatter);
        var request = new CommandCallRequest(
            "123",
            new[]
            {
                new ConsoleParameter("name", "Alice"),
                new ConsoleParameter("verbose", "true"),
                new ConsoleParameter("ratio", "2.5")
            });

        var args = _binder.Bind(caller, request);

        Assert.Equal(4, args.Length);
        Assert.Equal(123, args[0]);
        Assert.Equal("Alice", args[1]);
        Assert.Equal(true, args[2]);
        Assert.Equal(2.5, args[3]);
    }

    [Fact]
    public void Bind_PositionalHandler_WithFlag_NoValue_ReturnsTrue()
    {
        var caller = CommandCaller.CreateFor<PositionalHandler>(_formatter);
        var request = new CommandCallRequest(
            "123",
            new[]
            {
                new ConsoleParameter("name", "Alice"),
                new ConsoleParameter("verbose", null),
                new ConsoleParameter("ratio", "2.5")
            });

        var args = _binder.Bind(caller, request);

        Assert.Equal(4, args.Length);
        Assert.Equal(123, args[0]);
        Assert.Equal("Alice", args[1]);
        Assert.Equal(true, args[2]);
        Assert.Equal(2.5, args[3]);
    }

    [Fact]
    public void Bind_SampleHandler_AllParameters_ReturnsCorrectArray()
    {
        var caller = CommandCaller.CreateFor<SampleHandler>(_formatter);
        var request = new CommandCallRequest(
            null,
            new[]
            {
                new ConsoleParameter("id", "123"),
                new ConsoleParameter("name", "Alice"),
                new ConsoleParameter("verbose", "true"),
                new ConsoleParameter("ratio", "2.5")
            });

        var args = _binder.Bind(caller, request);

        Assert.Equal(4, args.Length);
        Assert.Equal(123, args[0]);
        Assert.Equal("Alice", args[1]);
        Assert.Equal(true, args[2]);
        Assert.Equal(2.5, args[3]);
    }

    [Fact]
    public void Bind_SampleHandler_MissingRequiredId_ThrowsIncorrectCommandStructureException()
    {
        var caller = CommandCaller.CreateFor<SampleHandler>(_formatter);
        var request = new CommandCallRequest(
            null,
            new[] { new ConsoleParameter("name", "Alice") }
        );

        Assert.Throws<IncorrectCommandStructureException>(() => _binder.Bind(caller, request));
    }

    [Fact]
    public void Bind_SampleHandler_WithFirstArgButMissingId_ThrowsIncorrectCommandStructureException()
    {
        var caller = CommandCaller.CreateFor<SampleHandler>(_formatter);
        var request = new CommandCallRequest(
            "123",
            new[] { new ConsoleParameter("name", "Alice") }
        );

        Assert.Throws<IncorrectCommandStructureException>(() => _binder.Bind(caller, request));
    }

    [Fact]
    public void Bind_PositionalHandler_WithOptionalParametersOmitted_UsesDefaults()
    {
        var caller = CommandCaller.CreateFor<PositionalHandler>(_formatter);
        var request = new CommandCallRequest(
            "123",
            new[] { new ConsoleParameter("name", "Alice") }
        );

        var args = _binder.Bind(caller, request);

        Assert.Equal(4, args.Length);
        Assert.Equal(123, args[0]);
        Assert.Equal("Alice", args[1]);
        Assert.Equal(false, args[2]); // default false
        Assert.Equal(1.5, args[3]);   // default 1.5
    }

    [Fact]
    public void Bind_InvalidParameterName_ThrowsInvalidParameterProvidedException()
    {
        var caller = CommandCaller.CreateFor<SampleHandler>(_formatter);
        var request = new CommandCallRequest(
            null,
            new[]
            {
                new ConsoleParameter("id", "123"),
                new ConsoleParameter("unknown", "value")
            });

        Assert.Throws<InvalidParameterProvidedException>(() => _binder.Bind(caller, request));
    }
}
