using Shopilent.Domain.Audit.DTOs;
using Shopilent.Domain.Audit.Enums;
using Shopilent.Domain.Common.Repositories.Read;

namespace Shopilent.Domain.Audit.Repositories.Read;

public interface IAuditLogReadRepository : IAggregateReadRepository<AuditLogDto>
{
    Task<IReadOnlyList<AuditLogDto>> GetByEntityAsync(string entityType, Guid entityId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>>
        GetByActionAsync(AuditAction action, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetRecentLogsAsync(int count, CancellationToken cancellationToken = default);
}