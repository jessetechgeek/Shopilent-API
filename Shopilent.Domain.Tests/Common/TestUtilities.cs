using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Tests.Common;

public static class TestUtilities
{
    /// <summary>
    /// Unwraps a Result and returns its value, or throws an exception if the Result is a failure.
    /// This is useful in tests where we expect a Result to be successful.
    /// </summary>
    public static T GetValue<T>(this Result<T> result)
    {
        if (result.IsFailure)
            throw new Exception($"Expected success result but got failure: {result.Error.Message}");

        return result.Value;
    }

    /// <summary>
    /// Asserts that a Result is successful.
    /// </summary>
    public static void AssertSuccess(this Result result)
    {
        Assert.True(result.IsSuccess, result.IsFailure ? $"Result failed with: {result.Error.Message}" : null);
    }

    /// <summary>
    /// Asserts that a Result is a failure with the expected error code.
    /// </summary>
    public static void AssertFailure(this Result result, string expectedErrorCode)
    {
        Assert.True(result.IsFailure, "Expected a failure result, but got success");
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }
}