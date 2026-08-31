using DiNet.HashSimilarityTK.CliToolkit.Core.Attributes;
using DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using System.Reflection;

namespace DiNet.HashSimilarityTK.CliToolkit.Core;

public class CommandCaller
{
    public record PrenamedParameterInfo(string FormattedName, ParameterInfo Parameter);

    public readonly ConstructorInfo Constructor;
    public readonly ParameterInfo? FirstUnflaggedParameter;
    public readonly PrenamedParameterInfo[] Parameters;
    public readonly Type Type;

    private CommandCaller(
        ConstructorInfo constructor,
        ParameterInfo? firstUnflaggedParameter,
        PrenamedParameterInfo[] parameters)
    {
        Constructor = constructor;
        FirstUnflaggedParameter = firstUnflaggedParameter;
        Parameters = parameters;
        Type = constructor.DeclaringType!;
    }



    /// <summary>
    /// Static factory method to generate valid caller metadata
    /// </summary>
    /// <typeparam name="T">Command type</typeparam>
    /// <param name="formatter">Formatter converts names of command constructor parameters</param>
    /// <returns>Caller metadata</returns>
    /// <exception cref="IncorrectCommandDefenitionException"></exception>
    public static CommandCaller CreateFor(Type type, INameFormatter formatter)
    {
        ConstructorInfo constructor = type.GetConstructors()
            .Single();

        ParameterInfo[] parameters = constructor.GetParameters();

        ParameterInfo? firstUnflagged = null;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].GetCustomAttribute<NotFlaggedAttribute>() != null)
            {
                firstUnflagged = parameters[i]; // only first will be cought, else exeption prevent execution

                if (i != 0)
                    throw new IncorrectCommandDefenitionException();
            }
        }

        var extraParameters = firstUnflagged == null
            ? parameters
            : parameters.Skip(1);

        var transformedParameters = extraParameters
            .Select(x => new PrenamedParameterInfo(formatter.Format(x.Name!), x))
            .ToArray();

        // assert if any duplicated names appeared
        var duplicates = transformedParameters
           .GroupBy(info => info.FormattedName)
           .Where(g => g.Count() > 1)
           .Select(g => g.Key)
           .ToArray();

        if (duplicates.Length > 0)
            throw new IncorrectCommandDefenitionException("Duplicated names after formatting");

        return new CommandCaller(constructor, firstUnflagged, transformedParameters);
    }

    public static CommandCaller CreateFor<T>(INameFormatter formatter)
    {
        return CreateFor(typeof(T), formatter);
    }
}
