using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Outbox;
using Shopilent.Domain.Outbox.DTOs;
using Shopilent.Domain.Outbox.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Outbox.Read;

public class OutboxMessageReadRepository : AggregateReadRepositoryBase<OutboxMessage, OutboxMessageDto>, IOutboxMessageReadRepository
{
    public OutboxMessageReadRepository(IDapperConnectionFactory connectionFactory, ILogger<OutboxMessageReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<OutboxMessageDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                type AS Type,
                content AS Content,
                processed_at AS ProcessedAt,
                error AS Error,
                retry_count AS RetryCount,
                scheduled_at AS ScheduledAt,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM outbox_messages
            WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<OutboxMessageDto>(sql, new { Id = id });
    }

    public override async Task<IReadOnlyList<OutboxMessageDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessageDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                type AS Type,
                content AS Content,
                processed_at AS ProcessedAt,
                error AS Error,
                retry_count AS RetryCount,
                scheduled_at AS ScheduledAt,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM outbox_messages
            ORDER BY created_at DESC";

        var result = await Connection.QueryAsync<OutboxMessageDto>(sql);
        return result.ToList();
    }

    public async Task<IReadOnlyList<OutboxMessageDto>> GetByStatusAsync(bool isProcessed, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                type AS Type,
                content AS Content,
                processed_at AS ProcessedAt,
                error AS Error,
                retry_count AS RetryCount,
                scheduled_at AS ScheduledAt,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM outbox_messages
            WHERE (processed_at IS NOT NULL) = @IsProcessed
            ORDER BY created_at DESC";

        var result = await Connection.QueryAsync<OutboxMessageDto>(sql, new { IsProcessed = isProcessed });
        return result.ToList();
    }

    public async Task<IReadOnlyList<OutboxMessageDto>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                type AS Type,
                content AS Content,
                processed_at AS ProcessedAt,
                error AS Error,
                retry_count AS RetryCount,
                scheduled_at AS ScheduledAt,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM outbox_messages
            WHERE type = @Type
            ORDER BY created_at DESC";

        var result = await Connection.QueryAsync<OutboxMessageDto>(sql, new { Type = type });
        return result.ToList();
    }

    public async Task<IReadOnlyList<OutboxMessageDto>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                type AS Type,
                content AS Content,
                processed_at AS ProcessedAt,
                error AS Error,
                retry_count AS RetryCount,
                scheduled_at AS ScheduledAt,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM outbox_messages
            WHERE created_at >= @From AND created_at <= @To
            ORDER BY created_at DESC";

        var result = await Connection.QueryAsync<OutboxMessageDto>(sql, new { From = from, To = to });
        return result.ToList();
    }

    public async Task<IReadOnlyList<OutboxMessageDto>> GetFailedMessagesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                id AS Id,
                type AS Type,
                content AS Content,
                processed_at AS ProcessedAt,
                error AS Error,
                retry_count AS RetryCount,
                scheduled_at AS ScheduledAt,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM outbox_messages
            WHERE error IS NOT NULL AND error != ''
            ORDER BY created_at DESC";

        var result = await Connection.QueryAsync<OutboxMessageDto>(sql);
        return result.ToList();
    }

    public async Task<IReadOnlyList<OutboxMessageDto>> GetUnprocessedMessagesAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT
                id AS Id,
                type AS Type,
                content AS Content,
                processed_at AS ProcessedAt,
                error AS Error,
                retry_count AS RetryCount,
                scheduled_at AS ScheduledAt,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM outbox_messages
            WHERE processed_at IS NULL
            ORDER BY scheduled_at ASC, created_at ASC";

        if (limit.HasValue)
        {
            sql += " LIMIT @Limit";
        }

        var result = await Connection.QueryAsync<OutboxMessageDto>(sql, new { Limit = limit });
        return result.ToList();
    }

    public async Task<int> GetUnprocessedCountAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM outbox_messages
            WHERE processed_at IS NULL";

        return await Connection.QuerySingleAsync<int>(sql);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM outbox_messages";

        return await Connection.QuerySingleAsync<int>(sql);
    }
}