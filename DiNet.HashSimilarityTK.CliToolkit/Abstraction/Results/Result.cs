using DiNet.HashSimilarityTK.CliToolkit.Abstraction.Results;

namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction.Results;

public abstract class ResultBase
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected ResultBase(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
}
public sealed class Result : ResultBase
{
    private Result(bool isSuccess, Error? error) : base(isSuccess, error) { }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(string message) => Failure(new Error(message));
    public static implicit operator Result(Exception ex) => Failure(ex);

}
public class Result<T> : ResultBase
{
    private readonly T? _value;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value of a failed result.");

    private Result(bool isSuccess, T? value, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
    public static implicit operator Result<T>(string errorMessage) => Failure(new Error(errorMessage));
    public static implicit operator Result<T>(Exception ex) => Failure(ex);

    public void Deconstruct(out bool isSuccess, out T? value, out Error? error)
    {
        isSuccess = IsSuccess;
        value = _value;
        error = Error;
    }
}
