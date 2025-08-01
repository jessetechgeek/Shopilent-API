using Meilisearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;
using Shopilent.Infrastructure.Search.Meilisearch.Settings;
using System.Text.Json;
using Index = Meilisearch.Index;

namespace Shopilent.Infrastructure.Search.Meilisearch.Services;

public class MeilisearchService : ISearchService
{
    private readonly MeilisearchClient _client;
    private readonly MeilisearchSettings _settings;
    private readonly ILogger<MeilisearchService> _logger;

    public MeilisearchService(
        IOptions<MeilisearchSettings> settings,
        ILogger<MeilisearchService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new MeilisearchClient(_settings.Url, _settings.ApiKey);
    }

    public async Task InitializeIndexesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var index = _client.Index(_settings.Indexes.Products);
            
            await index.UpdateSearchableAttributesAsync(new[]
            {
                "name",
                "description",
                "sku",
                "variant_skus",
                "categories.name",
                "attributes.value"
            });

            var currentFilterableAttrs = await index.GetFilterableAttributesAsync();
            var systemFilterableAttrs = new HashSet<string>
            {
                "category_ids",
                "price_range.min",
                "price_range.max", 
                "has_stock",
                "is_active",
                "status"
            };
            
            foreach (var attr in currentFilterableAttrs ?? [])
            {
                systemFilterableAttrs.Add(attr);
            }
            
            var missingSystemAttrs = new[] { "category_ids", "price_range.min", "price_range.max", "has_stock", "is_active", "status" }
                .Where(attr => !currentFilterableAttrs?.Contains(attr) == true);
            
            if (missingSystemAttrs.Any())
            {
                await index.UpdateFilterableAttributesAsync(systemFilterableAttrs.ToArray());
            }

            await index.UpdateSortableAttributesAsync(new[]
            {
                "name",
                "base_price",
                "created_at",
                "updated_at",
                "total_stock"
            });

            await index.UpdateDisplayedAttributesAsync(new[]
            {
                "*"
            });

            _logger.LogInformation("Meilisearch indexes initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Meilisearch indexes");
            throw;
        }
    }

    public async Task<Result> IndexProductAsync(ProductSearchDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            var index = _client.Index(_settings.Indexes.Products);
            var documentJson = JsonSerializer.Serialize(document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            
            var documentDict = JsonSerializer.Deserialize<Dictionary<string, object>>(documentJson);
            
            var newAttributeNames = new List<string>();
            if (documentDict?.ContainsKey("flat_attributes") == true && documentDict["flat_attributes"] is JsonElement flatAttributesElement)
            {
                if (flatAttributesElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in flatAttributesElement.EnumerateObject())
                    {
                        newAttributeNames.Add(property.Name);
                        
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            var values = property.Value.EnumerateArray().Select(v => v.GetString()).Where(v => !string.IsNullOrEmpty(v)).ToArray();
                            if (values.Length > 0)
                            {
                                documentDict[property.Name] = values;
                            }
                        }
                        else if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            var value = property.Value.GetString();
                            if (!string.IsNullOrEmpty(value))
                            {
                                documentDict[property.Name] = new[] { value };
                            }
                        }
                    }
                }
                documentDict.Remove("flat_attributes");
            }
            
            if (newAttributeNames.Any())
            {
                await EnsureAttributesAreFilterableAsync(index, newAttributeNames);
            }
            
            await index.AddDocumentsAsync(new[] { documentDict }, "id");
            
            _logger.LogDebug("Product {ProductId} indexed successfully", document.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index product {ProductId}", document.Id);
            return Result.Failure(Domain.Common.Errors.Error.Failure("Search.IndexFailed", "Failed to index product"));
        }
    }

    public async Task<Result> IndexProductsAsync(IEnumerable<ProductSearchDocument> documents, CancellationToken cancellationToken = default)
    {
        try
        {
            var documentList = documents.ToList();
            if (!documentList.Any())
                return Result.Success();

            var index = _client.Index(_settings.Indexes.Products);
            var batches = documentList.Chunk(_settings.BatchSize);

            var allNewAttributeNames = new HashSet<string>();
            
            foreach (var batch in batches)
            {
                var documentDicts = new List<Dictionary<string, object>>();
                
                foreach (var doc in batch)
                {
                    var documentJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    
                    var documentDict = JsonSerializer.Deserialize<Dictionary<string, object>>(documentJson);
                    
                    if (documentDict?.ContainsKey("flat_attributes") == true && documentDict["flat_attributes"] is JsonElement flatAttributesElement)
                    {
                        if (flatAttributesElement.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var property in flatAttributesElement.EnumerateObject())
                            {
                                allNewAttributeNames.Add(property.Name);
                                
                                if (property.Value.ValueKind == JsonValueKind.Array)
                                {
                                    var values = property.Value.EnumerateArray().Select(v => v.GetString()).Where(v => !string.IsNullOrEmpty(v)).ToArray();
                                    if (values.Length > 0)
                                    {
                                        documentDict[property.Name] = values;
                                    }
                                }
                                else if (property.Value.ValueKind == JsonValueKind.String)
                                {
                                    var value = property.Value.GetString();
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        documentDict[property.Name] = new[] { value };
                                    }
                                }
                            }
                        }
                        documentDict.Remove("flat_attributes");
                    }
                    
                    documentDicts.Add(documentDict);
                }
                
                await index.AddDocumentsAsync(documentDicts.ToArray(), "id");
            }
            
            if (allNewAttributeNames.Any())
            {
                await EnsureAttributesAreFilterableAsync(index, allNewAttributeNames);
            }

            _logger.LogInformation("Indexed {Count} products successfully", documentList.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index products batch");
            return Result.Failure(Domain.Common.Errors.Error.Failure("Search.BatchIndexFailed", "Failed to index products batch"));
        }
    }

    public async Task<Result> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var index = _client.Index(_settings.Indexes.Products);
            await index.DeleteOneDocumentAsync(productId.ToString());
            
            _logger.LogDebug("Product {ProductId} deleted from search index", productId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId} from search index", productId);
            return Result.Failure(Domain.Common.Errors.Error.Failure("Search.DeleteFailed", "Failed to delete product from search index"));
        }
    }

    public async Task<Shopilent.Domain.Common.Results.Result<SearchResponse<ProductSearchResultDto>>> SearchProductsAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var index = _client.Index(_settings.Indexes.Products);
            var searchParams = new SearchQuery();
            searchParams.Limit = request.PageSize;
            searchParams.Offset = (request.PageNumber - 1) * request.PageSize;
            searchParams.Filter = BuildSearchFilters(request);
            searchParams.Sort = BuildSortParameters(request.SortBy, request.SortDescending);

            var searchResult = await index.SearchAsync<Dictionary<string, object>>(request.Query, searchParams);
            
            var items = searchResult.Hits.Select(hit => MapToProductSearchResult(hit)).ToArray();
            var facets = new SearchFacets();
            
            var totalHits = searchResult.Hits.Count();
            var response = new SearchResponse<ProductSearchResultDto>
            {
                Items = items,
                Facets = facets,
                TotalCount = totalHits,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalHits / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber * request.PageSize < totalHits,
                Query = request.Query
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search products");
            return Result.Failure<SearchResponse<ProductSearchResultDto>>(
                Domain.Common.Errors.Error.Failure("Search.SearchFailed", "Failed to search products"));
        }
    }

    public async Task<Shopilent.Domain.Common.Results.Result<PaginatedResult<ProductDto>>> GetProductsAsync(ProductListingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchRequest = new SearchRequest
            {
                Query = request.SearchQuery,
                CategoryIds = request.CategoryIds.Length > 0 ? request.CategoryIds : 
                             request.CategoryId.HasValue ? new[] { request.CategoryId.Value } : [],
                AttributeFilters = request.AttributeFilters,
                PriceMin = request.PriceMin,
                PriceMax = request.PriceMax,
                InStockOnly = request.InStockOnly,
                ActiveOnly = request.IsActiveOnly,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortBy = MapSortColumn(request.SortColumn),
                SortDescending = request.SortDescending
            };

            var searchResult = await SearchProductsAsync(searchRequest, cancellationToken);
            if (searchResult.IsFailure)
                return Result.Failure<PaginatedResult<ProductDto>>(searchResult.Error);

            var products = searchResult.Value.Items.Select(MapToProductDto).ToArray();
            
            var paginatedResult = PaginatedResult<ProductDto>.Create(
                products,
                searchResult.Value.TotalCount,
                searchResult.Value.PageNumber,
                searchResult.Value.PageSize);

            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get products listing");
            return Result.Failure<PaginatedResult<ProductDto>>(
                Domain.Common.Errors.Error.Failure("Search.GetProductsFailed", "Failed to get products listing"));
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _client.HealthAsync();
            return health.Status == "available";
        }
        catch
        {
            return false;
        }
    }

    private string[] BuildSearchFilters(SearchRequest request)
    {
        var filters = new List<string>();

        if (request.ActiveOnly)
            filters.Add("is_active = true");

        if (request.CategoryIds.Length > 0)
        {
            var categoryFilter = string.Join(" OR ", request.CategoryIds.Select(id => $"category_ids = \"{id}\""));
            filters.Add($"({categoryFilter})");
        }

        if (request.AttributeFilters.Any())
        {
            foreach (var (attributeName, values) in request.AttributeFilters)
            {
                if (values.Length > 0)
                {
                    var flatAttributeName = attributeName.ToLowerInvariant();
                    var attributeFilter = string.Join(" OR ", values.Select(value => 
                        $"{flatAttributeName} = \"{value}\""));
                    filters.Add($"({attributeFilter})");
                }
            }
        }

        if (request.PriceMin.HasValue)
            filters.Add($"price_range.max >= {request.PriceMin.Value}");

        if (request.PriceMax.HasValue)
            filters.Add($"price_range.min <= {request.PriceMax.Value}");

        if (request.InStockOnly)
            filters.Add("has_stock = true");

        return filters.ToArray();
    }

    private string[] BuildSortParameters(string sortBy, bool descending)
    {
        var sortField = sortBy.ToLowerInvariant() switch
        {
            "name" => "name",
            "price" => "base_price",
            "created" => "created_at",
            "updated" => "updated_at",
            "stock" => "total_stock",
            _ => null
        };

        if (sortField == null)
            return Array.Empty<string>();

        var direction = descending ? "desc" : "asc";
        return new[] { $"{sortField}:{direction}" };
    }

    private string MapSortColumn(string sortColumn)
    {
        return sortColumn.ToLowerInvariant() switch
        {
            "name" => "name",
            "baseprice" => "price",
            "createdat" => "created",
            "updatedat" => "updated",
            _ => "relevance"
        };
    }

    private ProductSearchResultDto MapToProductSearchResult(Dictionary<string, object> hit)
    {
        var json = JsonSerializer.Serialize(hit);
        var result = JsonSerializer.Deserialize<ProductSearchResultDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        });
        
        return result ?? new ProductSearchResultDto();
    }

    private ProductDto MapToProductDto(ProductSearchResultDto searchResult)
    {
        return new ProductDto
        {
            Id = searchResult.Id,
            Name = searchResult.Name,
            Description = searchResult.Description,
            Sku = searchResult.SKU,
            Slug = searchResult.Slug,
            BasePrice = searchResult.BasePrice,
            IsActive = searchResult.IsActive,
            CreatedAt = searchResult.CreatedAt,
            UpdatedAt = searchResult.UpdatedAt,
        };
    }

    private SearchFacets MapFacets(IDictionary<string, IDictionary<string, long>>? facets)
    {
        if (facets == null)
            return new SearchFacets();

        var categoryFacets = new List<CategoryFacet>();
        var attributeFacets = new List<AttributeFacet>();

        foreach (var (facetName, facetValues) in facets)
        {
            if (facetName == "category_ids")
            {
                foreach (var (categoryId, count) in facetValues)
                {
                    if (Guid.TryParse(categoryId, out var id))
                    {
                        categoryFacets.Add(new CategoryFacet
                        {
                            Id = id,
                            Name = categoryId,
                            Count = (int)count
                        });
                    }
                }
            }
            else if (facetName == "attributes.name")
            {
                if (facets.ContainsKey("attributes.value"))
                {
                    var attributeValues = facets["attributes.value"];
                    var groupedAttributes = facetValues.GroupBy(kv => kv.Key)
                        .Select(g => new AttributeFacet
                        {
                            Name = g.Key,
                            Values = attributeValues.Where(av => av.Key.Contains(g.Key))
                                .Select(av => new AttributeValueFacet
                                {
                                    Value = av.Key,
                                    Count = (int)av.Value
                                }).ToArray()
                        });
                    
                    attributeFacets.AddRange(groupedAttributes);
                }
            }
        }

        return new SearchFacets
        {
            Categories = categoryFacets.ToArray(),
            Attributes = attributeFacets.ToArray(),
            PriceRange = new PriceRangeFacet()
        };
    }

    private async Task EnsureAttributesAreFilterableAsync(Index index, IEnumerable<string> attributeNames)
    {
        try
        {
            var currentFilterableAttrs = await index.GetFilterableAttributesAsync();
            var currentAttrsSet = new HashSet<string>(currentFilterableAttrs ?? []);
            var newAttrs = attributeNames.Where(attr => !currentAttrsSet.Contains(attr)).ToList();
            
            if (newAttrs.Any())
            {
                currentAttrsSet.UnionWith(newAttrs);
                await index.UpdateFilterableAttributesAsync(currentAttrsSet.ToArray());
                _logger.LogDebug("Added {Count} new filterable attributes: {Attributes}", 
                    newAttrs.Count, string.Join(", ", newAttrs));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update filterable attributes for: {Attributes}", 
                string.Join(", ", attributeNames));
        }
    }
}