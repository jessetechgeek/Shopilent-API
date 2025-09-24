using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Common.Exceptions;

public class ConcurrencyConflictException : Exception
{
    public Error Error { get; }

    public ConcurrencyConflictException() : base("A concurrency conflict occurred")
    {
        Error = Error.Conflict(
            code: "Concurrency.Conflict",
            message: "A concurrency conflict occurred. Please refresh and try again.");
    }

    public ConcurrencyConflictException(string message) : base(message)
    {
        Error = Error.Conflict(
            code: "Concurrency.Conflict",
            message: message);
    }

    public ConcurrencyConflictException(string message, Exception innerException) : base(message, innerException)
    {
        Error = Error.Conflict(
            code: "Concurrency.Conflict",
            message: message);
    }

    public ConcurrencyConflictException(string entityName, object entityId) 
        : base($"Concurrency conflict occurred while updating {entityName} with ID {entityId}")
    {
        Error = Error.Conflict(
            code: "Concurrency.Conflict",
            message: $"The {entityName} was modified by another process. Please refresh and try again.");
    }

    public ConcurrencyConflictException(string entityName, object entityId, Exception innerException)
        : base($"Concurrency conflict occurred while updating {entityName} with ID {entityId}", innerException)
    {
        Error = Error.Conflict(
            code: "Concurrency.Conflict", 
            message: $"The {entityName} was modified by another process. Please refresh and try again.");
    }

    public ConcurrencyConflictException(Error error) : base(error.Message)
    {
        Error = error;
    }

    public ConcurrencyConflictException(Error error, Exception innerException) : base(error.Message, innerException)
    {
        Error = error;
    }
}