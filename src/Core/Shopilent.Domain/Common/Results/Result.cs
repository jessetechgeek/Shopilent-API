using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Common.Results;

public class Result
{
    protected Result()
    {
        IsSuccess = true;
    }

    protected Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new Result();
    public static Result Failure(Error error) => new Result(error);
    public static Result<T> Success<T>(T value) => new Result<T>(value);
    public static Result<T> Failure<T>(Error error) => new Result<T>(error);

    public Result<T> ToResult<T>() => IsSuccess
        ? throw new InvalidOperationException("Cannot convert successful result to Result<T> without a value")
        : Result.Failure<T>(Error!);
}

