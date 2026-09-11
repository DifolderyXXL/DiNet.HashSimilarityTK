namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction.Results;

public static class ResultExtensions
{
    public static TResult Match<T, TResult>(this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error!);
    }

    public static Result<TResult> Map<T, TResult>(this Result<T> result, Func<T, TResult> map)
    {
        return result.IsSuccess
            ? Result<TResult>.Success(map(result.Value))
            : Result<TResult>.Failure(result.Error!);
    }

    public static Result<TResult> Bind<T, TResult>(this Result<T> result, Func<T, Result<TResult>> bind)
    {
        return result.IsSuccess ? bind(result.Value) : Result<TResult>.Failure(result.Error!);
    }

    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action) => result.Tap(action);
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure) action(result.Error!);
        return result;
    }

    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        if (result.IsSuccess && !predicate(result.Value))
            return Result<T>.Failure(error);
        return result;
    }
}