using Shopilent.Domain.Common.Repositories.Base.Write;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Domain.Sales.Repositories.Write;

public interface IOrderWriteRepository : IAggregateWriteRepository<Order>
{
    // EF Core will be used for reads in write repository as well
    Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<OrderItem> GetOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken = default);
}