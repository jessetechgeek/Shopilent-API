using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Repositories.Read;

namespace Shopilent.Domain.Catalog.Repositories.Read;

public interface ICategoryReadRepository : IAggregateReadRepository<CategoryDto>
{
    Task<CategoryDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetChildCategoriesAsync(Guid parentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetCategoryPathAsync(Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<CategoryDto>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    
    Task<DataTableResult<CategoryDetailDto>> GetCategoryDetailDataTableAsync(
        DataTableRequest request,
        CancellationToken cancellationToken = default);
}