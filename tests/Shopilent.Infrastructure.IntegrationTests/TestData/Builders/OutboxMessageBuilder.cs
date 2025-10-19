using Bogus;
using Shopilent.Domain.Outbox;
using System.Text.Json;

namespace Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

public class OutboxMessageBuilder
{
    private readonly Faker _faker = new();
    private string _type;
    private string _content;
    private DateTime? _scheduledAt;
    private DateTime? _createdAt;
    private object _messageData;

    public OutboxMessageBuilder()
    {
        // Set reasonable defaults
        _type = _faker.PickRandom("Event:UserCreated", "Event:ProductUpdated", "Event:OrderPlaced", "Email:WelcomeEmail");
        _scheduledAt = DateTime.UtcNow;
        
        // Default message data
        _messageData = new
        {
            Id = _faker.Random.Guid(),
            Name = _faker.Lorem.Word(),
            Timestamp = DateTime.UtcNow
        };
        _content = JsonSerializer.Serialize(_messageData);
    }

    public OutboxMessageBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public OutboxMessageBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public OutboxMessageBuilder WithMessage<T>(T message) where T : class
    {
        _messageData = message;
        _content = JsonSerializer.Serialize(message);
        _type = $"Event:{typeof(T).Name}";
        return this;
    }

    public OutboxMessageBuilder WithScheduledAt(DateTime scheduledAt)
    {
        _scheduledAt = scheduledAt;
        return this;
    }

    public OutboxMessageBuilder ScheduledInPast()
    {
        _scheduledAt = DateTime.UtcNow.AddMinutes(-_faker.Random.Int(5, 60));
        return this;
    }

    public OutboxMessageBuilder ScheduledInFuture()
    {
        _scheduledAt = DateTime.UtcNow.AddMinutes(_faker.Random.Int(5, 60));
        return this;
    }

    public OutboxMessageBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public OutboxMessageBuilder ForDomainEvent(string eventName, Guid entityId)
    {
        _messageData = new
        {
            EntityId = entityId,
            EventType = eventName,
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object>
            {
                ["Id"] = entityId,
                ["Name"] = _faker.Lorem.Word(),
                ["Status"] = _faker.PickRandom("Active", "Inactive", "Pending")
            }
        };
        // Note: _type will be overridden by OutboxMessage.Create() which uses SimplifyTypeName
        return this;
    }

    public OutboxMessageBuilder ForEmailNotification(string emailType, string recipientEmail)
    {
        _type = $"Email:{emailType}";
        _messageData = new
        {
            EmailType = emailType,
            RecipientEmail = recipientEmail,
            Subject = _faker.Lorem.Sentence(),
            Body = _faker.Lorem.Paragraphs(2),
            Timestamp = DateTime.UtcNow
        };
        _content = JsonSerializer.Serialize(_messageData);
        return this;
    }

    public OutboxMessage Build()
    {
        // Use reflection to create OutboxMessage since the constructor is private
        var outboxMessage = OutboxMessage.Create(_messageData, _scheduledAt);

        // Override fields if needed using reflection
        if (_createdAt.HasValue)
        {
            var createdAtField = typeof(OutboxMessage).BaseType.BaseType
                .GetField("<CreatedAt>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            createdAtField?.SetValue(outboxMessage, _createdAt.Value);
        }

        return outboxMessage;
    }

    public static OutboxMessage CreateDefault()
    {
        return new OutboxMessageBuilder().Build();
    }

    public static OutboxMessage CreateDomainEvent(string eventName, Guid entityId)
    {
        return new OutboxMessageBuilder()
            .ForDomainEvent(eventName, entityId)
            .Build();
    }

    public static OutboxMessage CreateEmailNotification(string emailType, string recipientEmail)
    {
        return new OutboxMessageBuilder()
            .ForEmailNotification(emailType, recipientEmail)
            .Build();
    }

    public static OutboxMessage CreateScheduledInPast()
    {
        return new OutboxMessageBuilder()
            .ScheduledInPast()
            .Build();
    }

    public static OutboxMessage CreateScheduledInFuture()
    {
        return new OutboxMessageBuilder()
            .ScheduledInFuture()
            .Build();
    }

    public static List<OutboxMessage> CreateMultipleUnprocessed(int count, DateTime? baseScheduledTime = null)
    {
        var messages = new List<OutboxMessage>();
        var baseTime = baseScheduledTime ?? DateTime.UtcNow.AddMinutes(-10);

        for (int i = 0; i < count; i++)
        {
            var scheduledTime = baseTime.AddSeconds(i * 2); // 2 seconds apart for ordering
            var message = new OutboxMessageBuilder()
                .WithScheduledAt(scheduledTime)
                .ForDomainEvent($"TestEvent{i}", Guid.NewGuid())
                .Build();
            messages.Add(message);
        }

        return messages;
    }

    public static List<OutboxMessage> CreateMixedProcessedAndUnprocessed(int totalCount, int processedCount)
    {
        var messages = new List<OutboxMessage>();
        var baseTime = DateTime.UtcNow.AddMinutes(-30);

        for (int i = 0; i < totalCount; i++)
        {
            var message = new OutboxMessageBuilder()
                .WithScheduledAt(baseTime.AddSeconds(i * 2))
                .ForDomainEvent($"MixedEvent{i}", Guid.NewGuid())
                .Build();

            // Mark first 'processedCount' messages as processed
            if (i < processedCount)
            {
                message.MarkAsProcessed();
            }

            messages.Add(message);
        }

        return messages;
    }

    public static OutboxMessage CreateWithError(string errorMessage)
    {
        var message = new OutboxMessageBuilder().Build();
        message.MarkAsFailed(errorMessage);
        return message;
    }

    public static OutboxMessage CreateProcessed()
    {
        var message = new OutboxMessageBuilder().Build();
        message.MarkAsProcessed();
        return message;
    }
}