using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Repositories.Base.Read;

namespace Shopilent.Domain.Catalog.Repositories.Read;

public interface IProductReadRepository : IAggregateReadRepository<ProductDto>
{
    Task<ProductDetailDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>> SearchAsync(string searchTerm, Guid? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);
}