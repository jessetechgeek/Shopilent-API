using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Identity.Write;

public class RefreshTokenWriteRepository : EntityWriteRepositoryBase<RefreshToken>, IRefreshTokenWriteRepository
{
    public RefreshTokenWriteRepository(ApplicationDbContext dbContext, ILogger<RefreshTokenWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<RefreshToken> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await DbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}