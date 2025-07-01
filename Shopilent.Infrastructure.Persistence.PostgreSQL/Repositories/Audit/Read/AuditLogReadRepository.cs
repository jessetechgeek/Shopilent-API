using Dapper;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Audit;
using Shopilent.Domain.Audit.DTOs;
using Shopilent.Domain.Audit.Enums;
using Shopilent.Domain.Audit.Repositories.Read;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Abstractions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Read;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Audit.Read;

public class AuditLogReadRepository : AggregateReadRepositoryBase<AuditLog, AuditLogDto>, IAuditLogReadRepository
{
    public AuditLogReadRepository(IDapperConnectionFactory connectionFactory, ILogger<AuditLogReadRepository> logger)
        : base(connectionFactory, logger)
    {
    }

    public override async Task<AuditLogDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                al.id AS Id,
                al.user_id AS UserId,
                u.first_name || ' ' || u.last_name AS UserName,
                u.email AS UserEmail,
                al.entity_type AS EntityType,
                al.entity_id AS EntityId,
                al.action AS Action,
                al.old_values AS OldValues,
                al.new_values AS NewValues,
                al.ip_address AS IpAddress,
                al.user_agent AS UserAgent,
                al.app_version AS AppVersion,
                al.created_at AS CreatedAt
            FROM audit_logs al
            LEFT JOIN users u ON al.user_id = u.id
            WHERE al.id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<AuditLogDto>(sql, new { Id = id });
    }

    public override async Task<IReadOnlyList<AuditLogDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                al.id AS Id,
                al.user_id AS UserId,
                u.first_name || ' ' || u.last_name AS UserName,
                u.email AS UserEmail,
                al.entity_type AS EntityType,
                al.entity_id AS EntityId,
                al.action AS Action,
                al.old_values AS OldValues,
                al.new_values AS NewValues,
                al.ip_address AS IpAddress,
                al.user_agent AS UserAgent,
                al.app_version AS AppVersion,
                al.created_at AS CreatedAt
            FROM audit_logs al
            LEFT JOIN users u ON al.user_id = u.id
            ORDER BY al.created_at DESC";

        var auditLogDtos = await Connection.QueryAsync<AuditLogDto>(sql);
        return auditLogDtos.ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByEntityAsync(string entityType, Guid entityId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                al.id AS Id,
                al.user_id AS UserId,
                u.first_name || ' ' || u.last_name AS UserName,
                u.email AS UserEmail,
                al.entity_type AS EntityType,
                al.entity_id AS EntityId,
                al.action AS Action,
                al.old_values AS OldValues,
                al.new_values AS NewValues,
                al.ip_address AS IpAddress,
                al.user_agent AS UserAgent,
                al.app_version AS AppVersion,
                al.created_at AS CreatedAt
            FROM audit_logs al
            LEFT JOIN users u ON al.user_id = u.id
            WHERE al.entity_type = @EntityType
            AND al.entity_id = @EntityId
            ORDER BY al.created_at DESC";

        var auditLogDtos = await Connection.QueryAsync<AuditLogDto>(
            sql, new { EntityType = entityType, EntityId = entityId });

        return auditLogDtos.ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                al.id AS Id,
                al.user_id AS UserId,
                u.first_name || ' ' || u.last_name AS UserName,
                u.email AS UserEmail,
                al.entity_type AS EntityType,
                al.entity_id AS EntityId,
                al.action AS Action,
                al.old_values AS OldValues,
                al.new_values AS NewValues,
                al.ip_address AS IpAddress,
                al.user_agent AS UserAgent,
                al.app_version AS AppVersion,
                al.created_at AS CreatedAt
            FROM audit_logs al
            LEFT JOIN users u ON al.user_id = u.id
            WHERE al.user_id = @UserId
            ORDER BY al.created_at DESC";

        var auditLogDtos = await Connection.QueryAsync<AuditLogDto>(sql, new { UserId = userId });
        return auditLogDtos.ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByActionAsync(AuditAction action,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                al.id AS Id,
                al.user_id AS UserId,
                u.first_name || ' ' || u.last_name AS UserName,
                u.email AS UserEmail,
                al.entity_type AS EntityType,
                al.entity_id AS EntityId,
                al.action AS Action,
                al.old_values AS OldValues,
                al.new_values AS NewValues,
                al.ip_address AS IpAddress,
                al.user_agent AS UserAgent,
                al.app_version AS AppVersion,
                al.created_at AS CreatedAt
            FROM audit_logs al
            LEFT JOIN users u ON al.user_id = u.id
            WHERE al.action = @Action
            ORDER BY al.created_at DESC";

        var auditLogDtos = await Connection.QueryAsync<AuditLogDto>(sql, new { Action = action.ToString() });
        return auditLogDtos.ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentLogsAsync(int count,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                al.id AS Id,
                al.user_id AS UserId,
                u.first_name || ' ' || u.last_name AS UserName,
                u.email AS UserEmail,
                al.entity_type AS EntityType,
                al.entity_id AS EntityId,
                al.action AS Action,
                al.old_values AS OldValues,
                al.new_values AS NewValues,
                al.ip_address AS IpAddress,
                al.user_agent AS UserAgent,
                al.app_version AS AppVersion,
                al.created_at AS CreatedAt
            FROM audit_logs al
            LEFT JOIN users u ON al.user_id = u.id
            ORDER BY al.created_at DESC
            LIMIT @Count";

        var auditLogDtos = await Connection.QueryAsync<AuditLogDto>(sql, new { Count = count });
        return auditLogDtos.ToList();
    }
}