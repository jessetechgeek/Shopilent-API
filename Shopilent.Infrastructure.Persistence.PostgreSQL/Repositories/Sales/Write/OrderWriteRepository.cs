using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Sales.Write;

public class OrderWriteRepository : AggregateWriteRepositoryBase<Order>, IOrderWriteRepository
{
    public OrderWriteRepository(ApplicationDbContext dbContext, ILogger<OrderWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderItem> GetOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken = default)
    {
        return await DbContext.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId, cancellationToken);
    }
}