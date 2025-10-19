using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Audit;
using Shopilent.Domain.Audit.Enums;
using Shopilent.Domain.Audit.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Audit.Write;

public class AuditLogWriteRepository : AggregateWriteRepositoryBase<AuditLog>, IAuditLogWriteRepository
{
    public AuditLogWriteRepository(ApplicationDbContext dbContext, ILogger<AuditLogWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<AuditLog> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.AuditLogs
            .FirstOrDefaultAsync(al => al.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.AuditLogs
            .Where(al => al.EntityType == entityType && al.EntityId == entityId)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.AuditLogs
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByActionAsync(AuditAction action,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.AuditLogs
            .Where(al => al.Action == action)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}