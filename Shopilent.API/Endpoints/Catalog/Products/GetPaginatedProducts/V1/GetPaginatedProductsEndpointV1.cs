using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetPaginatedProducts.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.GetPaginatedProducts.V1;

public class GetPaginatedProductsEndpointV1 :
    Endpoint<GetPaginatedProductsRequestV1, ApiResponse<GetPaginatedProductsResponseV1>>
{
    private readonly IMediator _mediator;

    public GetPaginatedProductsEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/products");
        AllowAnonymous(); 
        Description(b => b
            .WithName("GetPaginatedProducts")
            .Produces<ApiResponse<GetPaginatedProductsResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetPaginatedProductsResponseV1>>(StatusCodes.Status400BadRequest)
            .WithTags("Products"));
    }

    public override async Task HandleAsync(GetPaginatedProductsRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<GetPaginatedProductsResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var attributeFilters = ParseAttributeFiltersFromQuery();

        var query = new GetPaginatedProductsQueryV1
        {
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            SortColumn = req.SortColumn,
            SortDescending = req.SortDescending,
            CategoryId = req.CategoryId,
            IsActiveOnly = req.IsActiveOnly,
            SearchQuery = req.SearchQuery,
            AttributeFilters = attributeFilters,
            PriceMin = req.PriceMin,
            PriceMax = req.PriceMax,
            CategoryIds = req.CategoryIds,
            InStockOnly = req.InStockOnly
        };

        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<GetPaginatedProductsResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var paginatedResult = result.Value;
        var response = new GetPaginatedProductsResponseV1
        {
            Items = paginatedResult.Items,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize,
            TotalCount = paginatedResult.TotalCount,
            TotalPages = paginatedResult.TotalPages,
            HasPreviousPage = paginatedResult.HasPreviousPage,
            HasNextPage = paginatedResult.HasNextPage
        };

        var apiResponse = ApiResponse<GetPaginatedProductsResponseV1>.Success(
            response,
            "Products retrieved successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }

    private Dictionary<string, string[]> ParseAttributeFiltersFromQuery()
    {
        var attributeFilters = new Dictionary<string, string[]>();
        
        try
        {
            foreach (var kvp in HttpContext.Request.Query)
            {
                var key = kvp.Key;
                var values = kvp.Value.ToArray();

                if (key.StartsWith("attributeFilters[") && key.Contains("][") && key.EndsWith("]"))
                {
                    var start = key.IndexOf('[') + 1;
                    var end = key.IndexOf("][");
                    if (start > 0 && end > start)
                    {
                        var attributeName = key.Substring(start, end - start);
                        
                        if (!string.IsNullOrEmpty(attributeName))
                        {
                            if (!attributeFilters.ContainsKey(attributeName))
                                attributeFilters[attributeName] = Array.Empty<string>();

                            var existingValues = attributeFilters[attributeName].ToList();
                            existingValues.AddRange(values.Where(v => !string.IsNullOrEmpty(v) && !existingValues.Contains(v)));
                            attributeFilters[attributeName] = existingValues.ToArray();
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            return new Dictionary<string, string[]>();
        }

        return attributeFilters;
    }
}