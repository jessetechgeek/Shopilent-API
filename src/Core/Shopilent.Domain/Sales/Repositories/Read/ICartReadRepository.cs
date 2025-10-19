using Shopilent.Domain.Common.Repositories.Read;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Domain.Sales.Repositories.Read;

public interface ICartReadRepository : IAggregateReadRepository<CartDto>
{
    Task<CartDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CartDto>> GetAbandonedCartsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
}