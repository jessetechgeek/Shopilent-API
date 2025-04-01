using Shopilent.Domain.Common.Repositories.Base.Write;

namespace Shopilent.Domain.Sales.Repositories.Write;

public interface ICartWriteRepository : IAggregateWriteRepository<Cart>
{
    // EF Core will be used for reads in write repository as well
    Task<Cart> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cart> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}