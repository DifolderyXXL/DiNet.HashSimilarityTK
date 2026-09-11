using DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using System.ComponentModel;

namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

public class ParameterDeserializer : IParameterDeserializer
{
    public object? Deserialize(ParameterSource source, Type targetType)
    {
        if (source.Name != null && source.Value == null) // flag (--flag)
        {
            if (targetType != typeof(bool))
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
