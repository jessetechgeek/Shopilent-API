using Shopilent.Domain.Common.Repositories.Write;

namespace Shopilent.Domain.Catalog.Repositories.Write;

public interface IProductVariantWriteRepository : IEntityWriteRepository<ProductVariant>
{
    // EF Core will be used for reads in write repository
    Task<ProductVariant> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductVariant> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);
}