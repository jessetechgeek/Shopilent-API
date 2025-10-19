using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shopilent.Infrastructure.IntegrationTests.TestData.Utilities;

public static class LoggingTestHelpers
{
    public static TestLoggerProvider CreateTestLoggerProvider()
    {
        return new TestLoggerProvider();
    }

    public static IServiceCollection AddTestLogging(this IServiceCollection services)
    {
        var testLoggerProvider = new TestLoggerProvider();
        
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(testLoggerProvider);
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        services.AddSingleton(testLoggerProvider);
        
        return services;
    }


    public static bool HasLogEntry(this TestLoggerProvider provider, LogLevel logLevel, string messageContains)
    {
        return provider.GetLogs().Any(log => 
            log.LogLevel == logLevel && 
            log.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasLogEntry(this TestLoggerProvider provider, string categoryName, LogLevel logLevel, string messageContains)
    {
        return provider.GetLogs(categoryName).Any(log => 
            log.LogLevel == logLevel && 
            log.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
    }

    public static int CountLogEntries(this TestLoggerProvider provider, LogLevel logLevel)
    {
        return provider.GetLogs().Count(log => log.LogLevel == logLevel);
    }

    public static int CountLogEntries(this TestLoggerProvider provider, string categoryName, LogLevel logLevel)
    {
        return provider.GetLogs(categoryName).Count(log => log.LogLevel == logLevel);
    }

    public static LogEntry? GetLastLogEntry(this TestLoggerProvider provider)
    {
        return provider.GetLogs().OrderByDescending(log => log.Timestamp).FirstOrDefault();
    }

    public static LogEntry? GetLastLogEntry(this TestLoggerProvider provider, LogLevel logLevel)
    {
        return provider.GetLogs()
            .Where(log => log.LogLevel == logLevel)
            .OrderByDescending(log => log.Timestamp)
            .FirstOrDefault();
    }

    public static LogEntry? GetLastLogEntry(this TestLoggerProvider provider, string categoryName)
    {
        return provider.GetLogs(categoryName)
            .OrderByDescending(log => log.Timestamp)
            .FirstOrDefault();
    }

    public static LogEntry? GetFirstLogEntry(this TestLoggerProvider provider, LogLevel logLevel)
    {
        return provider.GetLogs()
            .Where(log => log.LogLevel == logLevel)
            .OrderBy(log => log.Timestamp)
            .FirstOrDefault();
    }

    public static IEnumerable<LogEntry> GetLogEntriesContaining(this TestLoggerProvider provider, string messageContains)
    {
        return provider.GetLogs()
            .Where(log => log.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<LogEntry> GetLogEntriesWithException(this TestLoggerProvider provider)
    {
        return provider.GetLogs().Where(log => log.Exception != null);
    }

    public static IEnumerable<LogEntry> GetLogEntriesWithException<TException>(this TestLoggerProvider provider)
        where TException : Exception
    {
        return provider.GetLogs().Where(log => log.Exception is TException);
    }

    public static bool HasExceptionLogEntry<TException>(this TestLoggerProvider provider, string messageContains = "")
        where TException : Exception
    {
        return provider.GetLogs().Any(log => 
            log.Exception is TException &&
            (string.IsNullOrEmpty(messageContains) || 
             log.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase)));
    }

    public static IEnumerable<LogEntry> GetLogEntriesBetween(this TestLoggerProvider provider, DateTime start, DateTime end)
    {
        return provider.GetLogs().Where(log => log.Timestamp >= start && log.Timestamp <= end);
    }

    public static IEnumerable<LogEntry> GetLogEntriesInLastSeconds(this TestLoggerProvider provider, int seconds)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-seconds);
        return provider.GetLogs().Where(log => log.Timestamp >= cutoff);
    }

    public static void AssertLogEntryExists(this TestLoggerProvider provider, LogLevel logLevel, string messageContains)
    {
        var hasEntry = provider.HasLogEntry(logLevel, messageContains);
        if (!hasEntry)
        {
            var allLogs = string.Join("\n", provider.GetLogs().Select(l => $"[{l.LogLevel}] {l.Message}"));
            throw new AssertionException(
                $"Expected log entry with level '{logLevel}' containing '{messageContains}' was not found.\n" +
                $"All logs:\n{allLogs}");
        }
    }

    public static void AssertLogEntryExists(this TestLoggerProvider provider, string categoryName, LogLevel logLevel, string messageContains)
    {
        var hasEntry = provider.HasLogEntry(categoryName, logLevel, messageContains);
        if (!hasEntry)
        {
            var categoryLogs = string.Join("\n", provider.GetLogs(categoryName).Select(l => $"[{l.LogLevel}] {l.Message}"));
            throw new AssertionException(
                $"Expected log entry in category '{categoryName}' with level '{logLevel}' containing '{messageContains}' was not found.\n" +
                $"Logs for category '{categoryName}':\n{categoryLogs}");
        }
    }

    public static void AssertLogEntryCount(this TestLoggerProvider provider, LogLevel logLevel, int expectedCount)
    {
        var actualCount = provider.CountLogEntries(logLevel);
        if (actualCount != expectedCount)
        {
            throw new AssertionException(
                $"Expected {expectedCount} log entries with level '{logLevel}', but found {actualCount}.");
        }
    }

    public static void AssertNoLogEntries(this TestLoggerProvider provider, LogLevel logLevel)
    {
        provider.AssertLogEntryCount(logLevel, 0);
    }

    public static void AssertNoLogEntriesContaining(this TestLoggerProvider provider, string messageContains)
    {
        var entries = provider.GetLogEntriesContaining(messageContains);
        if (entries.Any())
        {
            var messages = string.Join("\n", entries.Select(e => e.Message));
            throw new AssertionException(
                $"Expected no log entries containing '{messageContains}', but found:\n{messages}");
        }
    }
}

public class AssertionException : Exception
{
    public AssertionException(string message) : base(message)
    {
    }
}