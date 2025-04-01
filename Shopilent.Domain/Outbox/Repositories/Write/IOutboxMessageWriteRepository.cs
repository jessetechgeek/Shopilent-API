using Shopilent.Domain.Common.Repositories.Write;

namespace Shopilent.Domain.Outbox.Repositories.Write;

public interface IOutboxMessageWriteRepository : IWriteRepository<OutboxMessage>
{
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<OutboxMessage> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid id, string error, CancellationToken cancellationToken = default);
    Task<int> DeleteProcessedMessagesAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}