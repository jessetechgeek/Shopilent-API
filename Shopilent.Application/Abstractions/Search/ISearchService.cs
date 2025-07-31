using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Abstractions.Search;

public interface ISearchService
{
    Task InitializeIndexesAsync(CancellationToken cancellationToken = default);
    
    Task<Result> IndexProductAsync(ProductSearchDocument document, CancellationToken cancellationToken = default);
    
    Task<Result> IndexProductsAsync(IEnumerable<ProductSearchDocument> documents, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
    
    Task<Result<SearchResponse<ProductSearchResultDto>>> SearchProductsAsync(SearchRequest request, CancellationToken cancellationToken = default);
    
    Task<Result<PaginatedResult<ProductDto>>> GetProductsAsync(ProductListingRequest request, CancellationToken cancellationToken = default);
    
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}