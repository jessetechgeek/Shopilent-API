using Shopilent.Domain.Common.Repositories.Write;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.Domain.Shipping.Repositories.Write;

public interface IAddressWriteRepository : IAggregateWriteRepository<Address>
{
    // EF Core will be used for reads in write repository as well
    Task<Address> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Address> GetDefaultAddressAsync(Guid userId, AddressType addressType, CancellationToken cancellationToken = default);
}