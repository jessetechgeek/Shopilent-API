using Shopilent.Domain.Common.Repositories.Read;
using Shopilent.Domain.Outbox.DTOs;

namespace Shopilent.Domain.Outbox.Repositories.Read;

public interface IOutboxMessageReadRepository : IAggregateReadRepository<OutboxMessageDto>
{
    Task<IReadOnlyList<OutboxMessageDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessageDto>> GetByStatusAsync(bool isProcessed, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessageDto>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessageDto>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<OutboxMessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessageDto>> GetFailedMessagesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessageDto>> GetUnprocessedMessagesAsync(int? limit = null, CancellationToken cancellationToken = default);
    Task<int> GetUnprocessedCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}