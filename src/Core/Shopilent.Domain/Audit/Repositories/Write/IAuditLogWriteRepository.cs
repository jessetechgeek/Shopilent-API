using Shopilent.Domain.Audit.Enums;
using Shopilent.Domain.Common.Repositories.Write;

namespace Shopilent.Domain.Audit.Repositories.Write;

public interface IAuditLogWriteRepository : IAggregateWriteRepository<AuditLog>
{
    // EF Core will be used for reads in write repository as well
    Task<AuditLog> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByActionAsync(AuditAction action, CancellationToken cancellationToken = default);
}