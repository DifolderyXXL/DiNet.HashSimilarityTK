namespace DiNet.HashSimilarityTK.CliToolkit.Core.Models;

public record class CommandCallRequest(string? FirstUnflaggedArgument, ConsoleParameter[] Parameters);
