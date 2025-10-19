using Microsoft.EntityFrameworkCore;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Outbox;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Outbox.Write;

[Collection("IntegrationTests")]
public class OutboxMessageWriteRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public OutboxMessageWriteRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    #region Basic CRUD Operations

    [Fact]
    public async Task AddAsync_ValidOutboxMessage_ShouldPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();

        // Act
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(outboxMessage.Id);
        result.Type.Should().Be(outboxMessage.Type);
        result.Content.Should().Be(outboxMessage.Content);
        result.ProcessedAt.Should().BeNull();
        result.Error.Should().BeNull();
        result.RetryCount.Should().Be(0);
        result.CreatedAt.Should().BeCloseTo(outboxMessage.CreatedAt, TimeSpan.FromMilliseconds(100));
        result.ScheduledAt.Should().BeCloseTo(outboxMessage.ScheduledAt.Value, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingMessage_ShouldReturnMessage()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDomainEvent("UserCreated", Guid.NewGuid());
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(outboxMessage.Id);
        result.Type.Should().Be(outboxMessage.Type);
        result.Content.Should().Be(outboxMessage.Content);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentMessage_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingMessage_ShouldModifyMessage()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Detach to simulate real-world scenario
        DbContext.Entry(outboxMessage).State = EntityState.Detached;

        // Act - Load fresh entity and update
        var existingMessage = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        var errorMessage = "Test error occurred";
        existingMessage!.MarkAsFailed(errorMessage);

        await _unitOfWork.OutboxMessageWriter.UpdateAsync(existingMessage);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedMessage = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        updatedMessage.Should().NotBeNull();
        updatedMessage!.Error.Should().Be(errorMessage);
        updatedMessage.RetryCount.Should().Be(1);
        updatedMessage.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingMessage_ShouldRemoveFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.OutboxMessageWriter.DeleteAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        result.Should().BeNull();
    }

    #endregion

    #region Outbox-Specific Operations

    [Fact]
    public async Task GetUnprocessedMessagesAsync_HasUnprocessedMessages_ShouldReturnOrderedByScheduledAt()
    {
        // Arrange
        await ResetDatabaseAsync();

        var baseTime = DateTime.UtcNow.AddMinutes(-10);
        var messages = new List<OutboxMessage>
        {
            OutboxMessageBuilder.CreateDomainEvent("Event1", Guid.NewGuid()),
            OutboxMessageBuilder.CreateDomainEvent("Event2", Guid.NewGuid()),
            OutboxMessageBuilder.CreateDomainEvent("Event3", Guid.NewGuid())
        };

        // Set different scheduled times to test ordering
        var scheduledTimes = new[] { baseTime.AddMinutes(2), baseTime, baseTime.AddMinutes(1) };
        for (int i = 0; i < messages.Count; i++)
        {
            messages[i].Reschedule(scheduledTimes[i] - DateTime.UtcNow);
            await _unitOfWork.OutboxMessageWriter.AddAsync(messages[i]);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.OutboxMessageWriter.GetUnprocessedMessagesAsync(10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].ScheduledAt.Should().BeCloseTo(baseTime, TimeSpan.FromMilliseconds(100));
        result[1].ScheduledAt.Should().BeCloseTo(baseTime.AddMinutes(1), TimeSpan.FromMilliseconds(100));
        result[2].ScheduledAt.Should().BeCloseTo(baseTime.AddMinutes(2), TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_WithBatchSize_ShouldRespectLimit()
    {
        // Arrange
        await ResetDatabaseAsync();

        var messages = OutboxMessageBuilder.CreateMultipleUnprocessed(5, DateTime.UtcNow.AddMinutes(-5));
        foreach (var message in messages)
        {
            await _unitOfWork.OutboxMessageWriter.AddAsync(message);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.OutboxMessageWriter.GetUnprocessedMessagesAsync(3);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_FutureScheduledMessages_ShouldExcludeFutureMessages()
    {
        // Arrange
        await ResetDatabaseAsync();

        var pastMessage = OutboxMessageBuilder.CreateScheduledInPast();
        var futureMessage = OutboxMessageBuilder.CreateScheduledInFuture();

        await _unitOfWork.OutboxMessageWriter.AddAsync(pastMessage);
        await _unitOfWork.OutboxMessageWriter.AddAsync(futureMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.OutboxMessageWriter.GetUnprocessedMessagesAsync(10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(pastMessage.Id);
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_ProcessedMessages_ShouldExcludeProcessedMessages()
    {
        // Arrange
        await ResetDatabaseAsync();

        var unprocessedMessage = OutboxMessageBuilder.CreateDefault();
        var processedMessage = OutboxMessageBuilder.CreateProcessed();

        await _unitOfWork.OutboxMessageWriter.AddAsync(unprocessedMessage);
        await _unitOfWork.OutboxMessageWriter.AddAsync(processedMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.OutboxMessageWriter.GetUnprocessedMessagesAsync(10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(unprocessedMessage.Id);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ExistingMessage_ShouldUpdateProcessedAt()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.OutboxMessageWriter.MarkAsProcessedAsync(outboxMessage.Id);

        // Assert
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        result.Should().NotBeNull();
        result!.ProcessedAt.Should().NotBeNull();
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsProcessedAsync_NonExistentMessage_ShouldNotThrow()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var act = async () => await _unitOfWork.OutboxMessageWriter.MarkAsProcessedAsync(nonExistentId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MarkAsFailedAsync_ExistingMessage_ShouldUpdateErrorAndRetryCount()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        var errorMessage = "Processing failed due to network timeout";

        // Act
        await _unitOfWork.OutboxMessageWriter.MarkAsFailedAsync(outboxMessage.Id, errorMessage);

        // Assert
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        result.Should().NotBeNull();
        result!.Error.Should().Be(errorMessage);
        result.RetryCount.Should().Be(1);
        result.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsFailedAsync_MultipleFailures_ShouldIncrementRetryCount()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act - Mark as failed multiple times
        await _unitOfWork.OutboxMessageWriter.MarkAsFailedAsync(outboxMessage.Id, "First failure");
        await _unitOfWork.OutboxMessageWriter.MarkAsFailedAsync(outboxMessage.Id, "Second failure");
        await _unitOfWork.OutboxMessageWriter.MarkAsFailedAsync(outboxMessage.Id, "Third failure");

        // Assert
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        result.Should().NotBeNull();
        result!.Error.Should().Be("Third failure");
        result.RetryCount.Should().Be(3);
        result.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsFailedAsync_NonExistentMessage_ShouldNotThrow()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var act = async () => await _unitOfWork.OutboxMessageWriter.MarkAsFailedAsync(nonExistentId, "Error");
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Cleanup Operations

    [Fact]
    public async Task DeleteProcessedMessagesAsync_HasProcessedMessages_ShouldDeleteOldProcessedMessages()
    {
        // Arrange
        await ResetDatabaseAsync();

        var cutoffTime = DateTime.UtcNow.AddDays(-7);
        var oldProcessedMessage = OutboxMessageBuilder.CreateProcessed();
        var recentProcessedMessage = OutboxMessageBuilder.CreateProcessed();
        var unprocessedMessage = OutboxMessageBuilder.CreateDefault();

        // Set processed times
        oldProcessedMessage.MarkAsProcessed();
        SetProcessedAt(oldProcessedMessage, DateTime.UtcNow.AddDays(-10));

        recentProcessedMessage.MarkAsProcessed();
        SetProcessedAt(recentProcessedMessage, DateTime.UtcNow.AddDays(-3));

        await _unitOfWork.OutboxMessageWriter.AddAsync(oldProcessedMessage);
        await _unitOfWork.OutboxMessageWriter.AddAsync(recentProcessedMessage);
        await _unitOfWork.OutboxMessageWriter.AddAsync(unprocessedMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var deletedCount = await _unitOfWork.OutboxMessageWriter.DeleteProcessedMessagesAsync(cutoffTime);

        // Assert
        deletedCount.Should().Be(1);

        var oldResult = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(oldProcessedMessage.Id);
        oldResult.Should().BeNull();

        var recentResult = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(recentProcessedMessage.Id);
        recentResult.Should().NotBeNull();

        var unprocessedResult = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(unprocessedMessage.Id);
        unprocessedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteProcessedMessagesAsync_NoOldProcessedMessages_ShouldReturnZero()
    {
        // Arrange
        await ResetDatabaseAsync();

        var cutoffTime = DateTime.UtcNow.AddDays(-7);
        var recentProcessedMessage = OutboxMessageBuilder.CreateProcessed();
        var unprocessedMessage = OutboxMessageBuilder.CreateDefault();

        await _unitOfWork.OutboxMessageWriter.AddAsync(recentProcessedMessage);
        await _unitOfWork.OutboxMessageWriter.AddAsync(unprocessedMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var deletedCount = await _unitOfWork.OutboxMessageWriter.DeleteProcessedMessagesAsync(cutoffTime);

        // Assert
        deletedCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteProcessedMessagesAsync_EmptyRepository_ShouldReturnZero()
    {
        // Arrange
        await ResetDatabaseAsync();

        var cutoffTime = DateTime.UtcNow.AddDays(-7);

        // Act
        var deletedCount = await _unitOfWork.OutboxMessageWriter.DeleteProcessedMessagesAsync(cutoffTime);

        // Assert
        deletedCount.Should().Be(0);
    }

    #endregion

    #region Error Handling and Edge Cases

    [Fact]
    public async Task AddAsync_MultipleMessages_ShouldPersistAll()
    {
        // Arrange
        await ResetDatabaseAsync();

        var messages = new List<OutboxMessage>
        {
            OutboxMessageBuilder.CreateDomainEvent("Event1", Guid.NewGuid()),
            OutboxMessageBuilder.CreateDomainEvent("Event2", Guid.NewGuid()),
            OutboxMessageBuilder.CreateEmailNotification("Welcome", "test@example.com")
        };

        // Act
        foreach (var message in messages)
        {
            await _unitOfWork.OutboxMessageWriter.AddAsync(message);
        }
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var unprocessedMessages = await _unitOfWork.OutboxMessageWriter.GetUnprocessedMessagesAsync(10);
        unprocessedMessages.Should().HaveCount(3);

        // Verify by checking the Content field which contains our actual event data
        unprocessedMessages.Should().Contain(m => m.Content.Contains("Event1"));
        unprocessedMessages.Should().Contain(m => m.Content.Contains("Event2"));
        unprocessedMessages.Should().Contain(m => m.Content.Contains("Welcome"));
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_ZeroBatchSize_ShouldReturnEmpty()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.OutboxMessageWriter.GetUnprocessedMessagesAsync(0);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task MessageStateMutations_ShouldWorkCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Act - Test state transitions
        // 1. Mark as failed
        outboxMessage.MarkAsFailed("First error");
        await _unitOfWork.OutboxMessageWriter.UpdateAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        var afterFirstFailure = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        afterFirstFailure!.Error.Should().Be("First error");
        afterFirstFailure.RetryCount.Should().Be(1);

        // 2. Mark as failed again
        outboxMessage.MarkAsFailed("Second error");
        await _unitOfWork.OutboxMessageWriter.UpdateAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        var afterSecondFailure = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        afterSecondFailure!.Error.Should().Be("Second error");
        afterSecondFailure.RetryCount.Should().Be(2);

        // 3. Mark as processed
        outboxMessage.MarkAsProcessed();
        await _unitOfWork.OutboxMessageWriter.UpdateAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        var afterProcessed = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        afterProcessed!.ProcessedAt.Should().NotBeNull();
        afterProcessed.Error.Should().BeNull();
        afterProcessed.RetryCount.Should().Be(2); // Retry count should remain
    }

    [Fact]
    public async Task RescheduleMessage_ShouldUpdateScheduledAt()
    {
        // Arrange
        await ResetDatabaseAsync();

        var outboxMessage = OutboxMessageBuilder.CreateDefault();
        await _unitOfWork.OutboxMessageWriter.AddAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        var originalScheduledAt = outboxMessage.ScheduledAt;
        var delay = TimeSpan.FromHours(2);

        // Act
        outboxMessage.Reschedule(delay);
        await _unitOfWork.OutboxMessageWriter.UpdateAsync(outboxMessage);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.OutboxMessageWriter.GetByIdAsync(outboxMessage.Id);
        result.Should().NotBeNull();
        result!.ScheduledAt.Should().BeAfter(originalScheduledAt.Value);
        result.ScheduledAt.Should().BeCloseTo(DateTime.UtcNow.Add(delay), TimeSpan.FromSeconds(10));
    }

    #endregion

    #region Helper Methods

    private static void SetProcessedAt(OutboxMessage message, DateTime processedAt)
    {
        // Use reflection to set ProcessedAt for testing
        var field = typeof(OutboxMessage)
            .GetField("<ProcessedAt>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(message, processedAt);
    }


    #endregion
}
