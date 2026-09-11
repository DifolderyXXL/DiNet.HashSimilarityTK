using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.CliToolkit.Core;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using System.Reflection;
using System.Text;

namespace DiNet.HashSimilarityTK.CliToolkit.Services.HelpHandler;


public record HelpCommand : IQuery<bool>;
public class HelpCommandHandler(
    CommandRouteRegistry registry, 
    IQueryHandlerResolver queryHandlerResolver,
    INameFormatter formatter) : IQueryHandler<HelpCommand, bool>
{
    private const string Indent = "  ";
    public async Task<bool> Handle(HelpCommand query, CancellationToken ct)
    {
        foreach(var kvp in registry.Routes)
        {
            var caller = ResolveCaller(kvp.Value);

            Console.WriteLine(GetCommandHelpQuery(kvp.Key, caller));
        }

        return true;
    }

    private string GetCommandHelpQuery(string prefix, CommandCaller caller)
    {
        var sb = new StringBuilder();

        sb.AppendLine(prefix);

        if (caller.FirstUnflaggedParameter != null)
        {
            sb.Append(Indent);
            sb.AppendLine(ParameterInfoToHelper(caller.FirstUnflaggedParameter, true));
        }

        foreach(var p in caller.Parameters)
        {
            sb.Append(Indent);
            sb.Append($"--{p.FormattedName}");
            sb.Append(' ');
            sb.Append(ParameterInfoToHelper(p.Parameter, false));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ParameterInfoToHelper(ParameterInfo info, bool includeName)
    {
        var insert = info.IsOptional ? "(opt)" : string.Empty;
        var includeNameInsert = includeName ? $"[{info.Name}]" : string.Empty;
        return $"<{info.ParameterType.Name}>{insert}{includeNameInsert}";
    }

    private CommandCaller ResolveCaller(Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
        ?? throw new InvalidOperationException($"Type {handlerType.FullName} does not implement IQueryHandler<TQuery, TResponse>");

        var genericArgs = handlerInterface.GetGenericArguments();
        var queryType = genericArgs[0];

        return CommandCaller.CreateFor(queryType, formatter);
    }
}
