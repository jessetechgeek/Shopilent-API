using Shopilent.Domain.Common.Repositories.Base.Write;

namespace Shopilent.Domain.Catalog.Repositories.Write;

public interface IProductWriteRepository : IAggregateWriteRepository<Product>
{
    // EF Core will be used for reads in write repository as well
    Task<Product> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);
}