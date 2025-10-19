using Newtonsoft.Json;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Infrastructure.Cache.Redis.Models;

public class CachedResult<T>
{
    [JsonProperty] public bool IsSuccess { get; set; }

    [JsonProperty] public T Value { get; set; }

    [JsonProperty] public Error Error { get; set; }

    public static CachedResult<T> FromResult(Result<T> result)
    {
        return new CachedResult<T>
        {
            IsSuccess = result.IsSuccess,
            Value = result.IsSuccess ? result.Value : default,
            Error = result.IsFailure ? result.Error : null
        };
    }

    public Result<T> ToResult()
    {
        if (IsSuccess)
        {
            return Result.Success(Value);
        }
        else
        {
            return Result.Failure<T>(Error);
        }
    }
}