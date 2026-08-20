using System.ComponentModel;
using System.Reflection;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;


[Serializable]
public class FlagsCanOnlyBeAppliedToBooleanException : IncorrectCommandDefenitionException;

[Serializable]
public class IncorrectCommandStructureException : Exception;

[Serializable]
public class InvalidParameterProvidedException : IncorrectCommandStructureException;

[Serializable]
public class IncorrectCommandDefenitionException : Exception
{
    public IncorrectCommandDefenitionException()
    {
        
    }
    public IncorrectCommandDefenitionException(string message) : base(message)
    {
        
    }
}

public interface IParameterDeserializer
{
    object? Deserialize(ParameterSource source, Type targetType);
}

public class ParameterDeserializer : IParameterDeserializer
{
    public object? Deserialize(ParameterSource source, Type targetType)
    {
        if(source.Name != null && source.Value == null) // flag (--flag)
        {
            if(targetType != typeof(bool))
                throw new FlagsCanOnlyBeAppliedToBooleanException();

            return true;
        }

        if (source.Value == null)
        {
            // For nullable Null
            if (IsNullableType(targetType))
                return null;

            throw new ArgumentException($"Value is null but target type '{targetType.Name}' does not allow null.");
        }

        try
        {
            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromString(source.Value);
            }
            else
            {
                // fallback
                return Convert.ChangeType(source.Value, targetType);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert value '{source.Value}' to type '{targetType.Name}'.", ex);
        }
    }

    private static bool IsNullableType(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}

public record ParameterSource(string? Name, string? Value);

public record ConsoleParameter(string Name, string? Value);
public class CommandHandlerResolver(CommandCallerStore store, IParameterDeserializer serializer)
{
    public T Resolve<T>(CommandCallRequest request)
    {
        var caller = store.Get<T>() ?? throw new Exception("Handler does not exist");

        var parameters = new List<(object? value, int index)>();

        if(caller.FirstUnflaggedParameter != null)
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
                throw new InvalidParameterProvidedException();

            var info = parameterFromConstructor.Parameter;

            var isRequired = !info.IsOptional;
            if (isRequired && p == null)
                throw new IncorrectCommandStructureException();

            return (serializer.Deserialize(new(p.Name, p.Value), info.ParameterType), info.Position);
        });

        parameters.AddRange(additional);

        var addedPositions = parameters.Select(p => p.index).ToHashSet();

        foreach (var param in caller.Parameters)
        {
            int pos = param.Parameter.Position;
            if (!addedPositions.Contains(pos) && param.Parameter.IsOptional)
            {
                object? defaultValue = param.Parameter.DefaultValue;
                if (defaultValue == DBNull.Value) defaultValue = null;
                parameters.Add((defaultValue, pos));
            }
        }

        var arguments = parameters.OrderBy(x => x.index).ToArray();

        return (T?)Activator.CreateInstance(typeof(T), arguments) ?? throw new Exception("Cannot create an object");
    }
}

public class CommandCallerStore
{
    private readonly Dictionary<Type, CommandCaller> _callers = [];
    public CommandCaller? Get<T>()
    {
        if (_callers.TryGetValue(typeof(T), out var caller))
            return caller;

        return null;
    }
}

public class CommandCaller
{
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
    public static CommandCaller CreateFor<T>(ICommandNameFormatter formatter)
    {
        ConstructorInfo constructor = typeof(T).GetConstructors()
            .Single();

        ParameterInfo[] parameters = constructor.GetParameters();

        ParameterInfo? firstUnflagged = null;
        for (int i = 0; i < parameters.Length; i++)
        {
            if(parameters[i].GetCustomAttribute<NotFlaggedAttribute>() != null)
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
}

public record PrenamedParameterInfo(string FormattedName, ParameterInfo Parameter);

public interface ICommandNameFormatter
{
    public string Format(string rawName);
}


public record class CommandCallRequest(string? FirstUnflaggedArgument, ConsoleParameter[] Parameters);


public record Test([NotFlagged] int a, int? b);

[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
sealed class NotFlaggedAttribute : Attribute
{ }



/*
 1. First parameter unflagged 
    = Maximum One
    = Only first
 
 */