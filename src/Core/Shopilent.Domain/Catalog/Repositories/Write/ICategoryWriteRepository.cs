using Shopilent.Domain.Common.Repositories.Write;

namespace Shopilent.Domain.Catalog.Repositories.Write;

public interface ICategoryWriteRepository : IAggregateWriteRepository<Category>
{
    // EF Core will be used for reads in write repository as well
    Task<Category> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}