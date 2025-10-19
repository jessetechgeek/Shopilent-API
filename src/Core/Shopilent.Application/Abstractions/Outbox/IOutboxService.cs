namespace Shopilent.Application.Abstractions.Outbox;

public interface IOutboxService
{
    Task PublishAsync<T>(T message, DateTime? scheduledAt = null, CancellationToken cancellationToken = default) where T : class;
    Task ProcessMessagesAsync(CancellationToken cancellationToken = default);
    Task CleanupOldMessagesAsync(int daysToKeep = 7, CancellationToken cancellationToken = default);
}