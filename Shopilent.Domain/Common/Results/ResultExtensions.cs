namespace Shopilent.Domain.Common.Results;

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T value) => Result.Success(value);

    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
        => result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TOut>(result.Error!);

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error!);

        var mapped = await mapper(result.Value);
        return Result.Success(mapped);
    }

    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
        => result.IsSuccess
            ? binder(result.Value)
            : Result.Failure<TOut>(result.Error!);

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error!);

        return await binder(result.Value);
    }
}