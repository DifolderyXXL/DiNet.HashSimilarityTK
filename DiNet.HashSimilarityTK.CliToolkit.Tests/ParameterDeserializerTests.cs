using DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

namespace DiNet.HashSimilarityTK.CliToolkit.Tests;

public class ParameterDeserializerTests
{
    private readonly ParameterDeserializer _deserializer = new();


    [Fact]
    public void FromInt_ValidString_ReturnsInt()
    {
        var result = _deserializer.Deserialize(new ParameterSource(null, "123"), typeof(int));
        Assert.Equal(123, result);
    }

    [Fact]
    public void FromFlag_Bool_ReturnsTrue()
    {
        var result = _deserializer.Deserialize(new ParameterSource("verbose", null), typeof(bool));
        Assert.True((bool)result!);
    }

    [Fact]
    public void FromFlag_NonBool_ThrowsFlagsCanOnlyBeAppliedToBooleanException()
    {
        Assert.Throws<FlagsCanOnlyBeAppliedToBooleanException>(
            () => _deserializer.Deserialize(new ParameterSource("verbose", null), typeof(int)));
    }


    public static TheoryData<string, Type, object> ValidConversions => new()
    {
        { "123", typeof(int), 123 },
        { "123.45", typeof(double), 123.45 },
        { "true", typeof(bool), true },
        { "hello", typeof(string), "hello" },
        { "2024-12-25", typeof(DateTime), new DateTime(2024, 12, 25) },
        { "Red", typeof(ConsoleColor), ConsoleColor.Red },
        { "99.99", typeof(decimal), 99.99m },
        { "9223372036854775807", typeof(long), long.MaxValue },
        { "3.14", typeof(float), 3.14f },
    };

    [Theory]
    [MemberData(nameof(ValidConversions))]
    public void FromValidString_ReturnsExpected(string value, Type targetType, object expected)
    {
        var result = _deserializer.Deserialize(new ParameterSource(null, value), targetType);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(double))]
    public void FromNull_NonNullableType_ThrowsArgumentException(Type targetType)
    {
        Assert.Throws<ArgumentException>(
            () => _deserializer.Deserialize(new ParameterSource(null, null), targetType));
    }

    [Theory]
    [InlineData(typeof(int?))]
    [InlineData(typeof(bool?))]
    [InlineData(typeof(string))]
    public void FromNull_NullableOrReference_ReturnsNull(Type targetType)
    {
        var result = _deserializer.Deserialize(new ParameterSource(null, null), targetType);
        Assert.Null(result);
    }
}
