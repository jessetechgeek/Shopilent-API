namespace Shopilent.Domain.Common.Errors;

public record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public IReadOnlyDictionary<string, string[]>? Metadata { get; }

    internal Error(
        string code,
        string message,
        ErrorType type = ErrorType.Failure,
        Dictionary<string, string[]>? metadata = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata;
    }

    public static Error Failure(
        string code = "Failure",
        string message = "A failure has occurred.")
        => new(code, message);
    
    public static Error NotFound(
        string code = "NotFound",
        string message = "The requested resource was not found.")
        => new(code, message, ErrorType.NotFound);

    public static Error Validation(
        string code = "ValidationError",
        string message = "A validation error occurred.",
        Dictionary<string, string[]>? metadata = null)
        => new(code, message, ErrorType.Validation, metadata);

    public static Error Conflict(
        string code = "Conflict",
        string message = "A conflict error occurred.")
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(
        string code = "Unauthorized",
        string message = "You are not authorized to perform this action.")
        => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(
        string code = "Forbidden",
        string message = "You are forbidden to perform this action.")
        => new(code, message, ErrorType.Forbidden);
}