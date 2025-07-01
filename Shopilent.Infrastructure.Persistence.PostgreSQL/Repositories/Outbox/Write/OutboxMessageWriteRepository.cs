using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Outbox;
using Shopilent.Domain.Outbox.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Outbox.Write;

public class OutboxMessageWriteRepository : EntityWriteRepositoryBase<OutboxMessage>, IOutboxMessageWriteRepository
{
    public OutboxMessageWriteRepository(ApplicationDbContext dbContext, ILogger<OutboxMessageWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<OutboxMessage> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.OutboxMessages
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.OutboxMessages
            .Where(o => o.ProcessedAt == null && o.ScheduledAt <= DateTime.UtcNow)
            .OrderBy(o => o.ScheduledAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await GetByIdAsync(id, cancellationToken);
        if (message != null)
        {
            message.MarkAsProcessed();
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid id, string error, CancellationToken cancellationToken = default)
    {
        var message = await GetByIdAsync(id, cancellationToken);
        if (message != null)
        {
            message.MarkAsFailed(error);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> DeleteProcessedMessagesAsync(DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        var messagesToDelete = await DbContext.OutboxMessages
            .Where(o => o.ProcessedAt != null && o.ProcessedAt < olderThan)
            .ToListAsync(cancellationToken);

        DbContext.OutboxMessages.RemoveRange(messagesToDelete);
        await DbContext.SaveChangesAsync(cancellationToken);

        return messagesToDelete.Count;
    }
}