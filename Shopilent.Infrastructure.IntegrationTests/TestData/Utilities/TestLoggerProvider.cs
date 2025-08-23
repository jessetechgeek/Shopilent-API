using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Shopilent.Infrastructure.IntegrationTests.TestData.Utilities;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, TestLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new TestLogger(name));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }

    public IEnumerable<LogEntry> GetLogs()
    {
        return _loggers.Values.SelectMany(logger => logger.LogEntries);
    }

    public IEnumerable<LogEntry> GetLogs(string categoryName)
    {
        return _loggers.TryGetValue(categoryName, out var logger) 
            ? logger.LogEntries 
            : Enumerable.Empty<LogEntry>();
    }

    public void ClearLogs()
    {
        foreach (var logger in _loggers.Values)
        {
            logger.ClearLogs();
        }
    }
}

public class TestLogger : ILogger
{
    private readonly string _categoryName;
    private readonly List<LogEntry> _logEntries = new();

    public TestLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IReadOnlyList<LogEntry> LogEntries => _logEntries.AsReadOnly();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        var logEntry = new LogEntry
        {
            CategoryName = _categoryName,
            LogLevel = logLevel,
            EventId = eventId,
            Message = message,
            Exception = exception,
            State = state,
            Timestamp = DateTime.UtcNow
        };

        _logEntries.Add(logEntry);
    }

    public void ClearLogs()
    {
        _logEntries.Clear();
    }
}

public class LogEntry
{
    public string CategoryName { get; set; } = string.Empty;
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public object? State { get; set; }
    public DateTime Timestamp { get; set; }
}