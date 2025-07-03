namespace Shopilent.API.Common.Models;

public class ApiResponse<T> where T : class
{
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public T Data { get; set; }
    public string[] Errors { get; set; }

    public ApiResponse()
    {
        Succeeded = true;
        Message = string.Empty;
        Errors = Array.Empty<string>();
    }

    public static ApiResponse<T> Success(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> Failure(string error, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = error,
            Errors = new[] { error },
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Failure(string[] errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = errors.FirstOrDefault() ?? "An error occurred",
            Errors = errors,
            StatusCode = statusCode
        };
    }
}