using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Repositories.Write;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Common.Write;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Write;

public class ProductVariantWriteRepository : EntityWriteRepositoryBase<ProductVariant>, IProductVariantWriteRepository
{
    public ProductVariantWriteRepository(ApplicationDbContext dbContext, ILogger<ProductVariantWriteRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<ProductVariant> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.ProductVariants
            .Include(pv => pv.VariantAttributes)
            .FirstOrDefaultAsync(pv => pv.Id == id, cancellationToken);
    }

    public async Task<ProductVariant> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        return await DbContext.ProductVariants
            .Include(pv => pv.VariantAttributes)
            .FirstOrDefaultAsync(pv => pv.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.ProductVariants
            .Include(pv => pv.VariantAttributes)
            .Where(pv => pv.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        var query = DbContext.ProductVariants.Where(pv => pv.Sku == sku);

        if (excludeId.HasValue)
        {
            query = query.Where(pv => pv.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}