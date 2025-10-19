using Microsoft.Extensions.Logging;

namespace Shopilent.Infrastructure.Logging.Extensions;

public static class LoggerExtensions
{
    public static void LogUserAction(this ILogger logger, string action, string userId, string? details = null)
    {
        logger.LogInformation(
            "User Action: {Action} by User {UserId} {Details}",
            action,
            userId,
            details ?? string.Empty);
    }

    public static void LogDatabaseOperation(this ILogger logger, string operation, string entity,
        string? details = null)
    {
        logger.LogInformation(
            "Database Operation: {Operation} on {Entity} {Details}",
            operation,
            entity,
            details ?? string.Empty);
    }

    public static void LogSecurityEvent(this ILogger logger, string eventType, string userId, string? details = null)
    {
        logger.LogWarning(
            "Security Event: {EventType} for User {UserId} {Details}",
            eventType,
            userId,
            details ?? string.Empty);
    }

    // Added more specific methods for common scenarios
    public static void LogLoginAttempt(this ILogger logger, string username, bool success, string? ipAddress = null)
    {
        if (success)
        {
            logger.LogInformation(
                "Successful login attempt for user {Username} from {IpAddress}",
                username,
                ipAddress ?? "unknown IP");
        }
        else
        {
            logger.LogWarning(
                "Failed login attempt for user {Username} from {IpAddress}",
                username,
                ipAddress ?? "unknown IP");
        }
    }

    public static void LogApiRequest(this ILogger logger, string endpoint, string method, int statusCode,
        long durationMs)
    {
        var level = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        logger.Log(level,
            "API {Method} request to {Endpoint} completed with status {StatusCode} in {Duration}ms",
            method,
            endpoint,
            statusCode,
            durationMs);
    }

    public static void LogDataAccess(this ILogger logger, string operation, string entity, bool success,
        long durationMs)
    {
        if (success)
        {
            logger.LogInformation(
                "Data access {Operation} on {Entity} completed successfully in {Duration}ms",
                operation,
                entity,
                durationMs);
        }
        else
        {
            logger.LogError(
                "Data access {Operation} on {Entity} failed after {Duration}ms",
                operation,
                entity,
                durationMs);
        }
    }

    public static void LogPermissionCheck(this ILogger logger, string userId, string resource, string action,
        bool allowed)
    {
        if (allowed)
        {
            logger.LogDebug(
                "Permission granted for user {UserId} to {Action} on {Resource}",
                userId,
                action,
                resource);
        }
        else
        {
            logger.LogWarning(
                "Permission denied for user {UserId} to {Action} on {Resource}",
                userId,
                action,
                resource);
        }
    }

    public static void LogException(this ILogger logger, Exception exception, string? context = null)
    {
        logger.LogError(
            exception,
            "Exception occurred{Context}: {ExceptionMessage}",
            !string.IsNullOrEmpty(context) ? $" during {context}" : string.Empty,
            exception.Message);
    }
}