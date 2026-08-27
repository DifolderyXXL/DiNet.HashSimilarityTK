using DiNet.HashSimilarityTK.CliToolkit.Core;
using DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Core.Models;

namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

public class ParameterBinder(IParameterDeserializer serializer) : IParameterBinder
{
    public object?[] Bind(CommandCaller caller, CommandCallRequest request)
    {
        var parameters = new List<(object? value, int index)>();

        if (caller.FirstUnflaggedParameter != null)
        {
            var isRequired = !caller.FirstUnflaggedParameter.IsOptional;
            if (isRequired && request.FirstUnflaggedArgument == null)
                throw new IncorrectCommandStructureException();

            parameters.Add((serializer.Deserialize(new(
                null,
                request.FirstUnflaggedArgument
                ), caller.FirstUnflaggedParameter.ParameterType), 0));
        }

        var additional = request.Parameters.Select(p =>
        {
            var parameterFromConstructor = caller.Parameters.FirstOrDefault(x => string.Equals(x.FormattedName, p.Name));

            if (parameterFromConstructor == null)
                throw new InvalidParameterProvidedException($"Name: '{p.Name}'; value: '{p.Value}'");

            var info = parameterFromConstructor.Parameter;

            //var isRequired = !info.IsOptional;
            //if (isRequired && p.Value == null)
            //    throw new IncorrectCommandStructureException();

            return (serializer.Deserialize(new(p.Name, p.Value), info.ParameterType), info.Position);
        });

        parameters.AddRange(additional);

        var addedPositions = parameters.Select(p => p.index).ToHashSet();

        foreach (var param in caller.Parameters)
        {
            int pos = param.Parameter.Position;
            if (!addedPositions.Contains(pos))
            {
                if (param.Parameter.IsOptional)
                {
                    object? defaultValue = param.Parameter.DefaultValue;
                    if (defaultValue == DBNull.Value) defaultValue = null;
                    parameters.Add((defaultValue, pos));
                }
                else
                {
                    throw new IncorrectCommandStructureException();
                }
            }
        }

        return parameters.OrderBy(x => x.index).Select(x => x.value).ToArray();
    }
}
