using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DiNet.HashSimilarityTK.Cli;

public record CalculateHashQuery(string FilePath, string Algorithm = "SHA256") : IQuery<HashResultResponse>;

public record HashResultResponse(string FilePath, string Hash, string Algorithm);

public class CalculateHashQueryHandler : IQueryHandler<CalculateHashQuery, HashResultResponse>
{
    public async Task<HashResultResponse> Handle(CalculateHashQuery query, CancellationToken ct)
    {


        return new HashResultResponse(query.FilePath, "HASHTRING", query.Algorithm.ToUpperInvariant());
    }
}

public class HashResultResponsePresenter : IDataPresenter<HashResultResponse>
{
    public void Present(HashResultResponse response)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{response.Algorithm}] ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{response.FilePath}: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(response.Hash);
        Console.ResetColor();
    }
}