using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Abstractions.Search;

public interface ISearchService
{
    Task InitializeIndexesAsync(CancellationToken cancellationToken = default);

    Task<Result> IndexProductAsync(ProductSearchDocument document, CancellationToken cancellationToken = default);

    Task<Result> IndexProductsAsync(IEnumerable<ProductSearchDocument> documents, CancellationToken cancellationToken = default);

    Task<Result> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<Result> DeleteProductsByIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<Guid>>> GetAllProductIdsAsync(CancellationToken cancellationToken = default);

    Task<Result<SearchResponse<ProductSearchResultDto>>> SearchProductsAsync(SearchRequest request, CancellationToken cancellationToken = default);

    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
