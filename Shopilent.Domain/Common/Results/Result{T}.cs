using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Common.Results;

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T value) : base()
    {
        _value = value;
    }

    protected internal Result(Error error) : base(error)
    {
        _value = default;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result");

    public static implicit operator T(Result<T> result) => result.Value;
}