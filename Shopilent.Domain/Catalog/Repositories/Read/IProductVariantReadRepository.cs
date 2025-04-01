using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Repositories.Base.Read;

namespace Shopilent.Domain.Catalog.Repositories.Read;

public interface IProductVariantReadRepository : IEntityReadRepository<ProductVariantDto>
{
    Task<IReadOnlyList<ProductVariantDto>> GetByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default);

    Task<ProductVariantDto> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductVariantDto>> GetInStockVariantsAsync(Guid productId,
        CancellationToken cancellationToken = default);
}