using Shopilent.Domain.Common.Repositories.Base.Read;
using Shopilent.Domain.Identity.DTOs;

namespace Shopilent.Domain.Identity.Repositories.Read;

public interface IRefreshTokenReadRepository : IEntityReadRepository<RefreshTokenDto>
{
    Task<RefreshTokenDto> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshTokenDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshTokenDto>> GetActiveTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}