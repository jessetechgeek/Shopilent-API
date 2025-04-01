using Shopilent.Domain.Common.Repositories.Base.Write;

namespace Shopilent.Domain.Identity.Repositories.Write;

public interface IRefreshTokenWriteRepository : IEntityWriteRepository<RefreshToken>
{
    // EF Core will be used for reads in write repository
    Task<RefreshToken> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}