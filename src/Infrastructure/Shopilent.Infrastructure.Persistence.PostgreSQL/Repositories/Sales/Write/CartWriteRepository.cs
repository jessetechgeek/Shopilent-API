using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Sales.Write;

public class CartWriteRepository : AggregateWriteRepositoryBase<Cart>, ICartWriteRepository
{
    public CartWriteRepository(ApplicationDbContext dbContext, ILogger<CartWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<Cart> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cart> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Carts
            .Include(c => c.Items)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Cart> GetCartByItemIdAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Carts
            .Include(c => c.Items)
            .Where(c => c.Items.Any(i => i.Id == cartItemId))
            .FirstOrDefaultAsync(cancellationToken);
    }
}