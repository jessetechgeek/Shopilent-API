using System.Text.Json;
using Shopilent.Domain.Common;

namespace Shopilent.Domain.Outbox;

public class OutboxMessage : AggregateRoot
{
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string Error { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? ScheduledAt { get; private set; }

    private OutboxMessage()
    {
        // Required by EF Core
    }

    private OutboxMessage(string type, string content, DateTime? scheduledAt = null)
    {
        Type = type;
        Content = content;
        ProcessedAt = null;
        Error = null;
        RetryCount = 0;
        ScheduledAt = scheduledAt ?? DateTime.UtcNow;
    }

    public static OutboxMessage Create<T>(T message, DateTime? scheduledAt = null) where T : class
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        // Store the type information in a simplified way
        var type = SimplifyTypeName(message.GetType());
        var content = JsonSerializer.Serialize(message);

        return new OutboxMessage(type, content, scheduledAt);
    }

    public T GetMessage<T>() where T : class
    {
        return JsonSerializer.Deserialize<T>(Content);
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
        RetryCount++;
    }

    public void Reschedule(TimeSpan delay)
    {
        ScheduledAt = DateTime.UtcNow.Add(delay);
    }

    private static string SimplifyTypeName(Type type)
    {
        // Check if it's a generic type (like DomainEventNotification<>)
        if (type.IsGenericType)
        {
            // Get the generic argument type (actual event)
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length > 0 &&
                type.Name.Contains("DomainEventNotification") &&
                genericArgs[0].Namespace?.Contains("Events") == true)
            {
                // For domain events, store just the event type name with a prefix
                return $"Event:{genericArgs[0].Name}";
            }
        }

        // For other types, use the full name
        return type.FullName;
    }
}