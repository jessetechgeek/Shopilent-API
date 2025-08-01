using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Search.Queries.UniversalSearch.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Search.UniversalSearch.V1;

public class UniversalSearchEndpointV1 : Endpoint<UniversalSearchRequestV1, ApiResponse<UniversalSearchResponseV1>>
{
    private readonly ISender _sender;

    public UniversalSearchEndpointV1(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("v1/search");
        AllowAnonymous();
        Description(b => b
            .WithName("UniversalSearch")
            .Produces<ApiResponse<UniversalSearchResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UniversalSearchResponseV1>>(StatusCodes.Status400BadRequest)
            .WithTags("Search"));
    }

    public override async Task HandleAsync(UniversalSearchRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UniversalSearchResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Parse attribute filters from query string manually since FastEndpoints doesn't handle complex nested parameters
        var attributeFilters = ParseAttributeFiltersFromQuery();

        var query = new UniversalSearchQueryV1(
            req.Query,
            req.CategoryIds,
            attributeFilters,
            req.PriceMin,
            req.PriceMax,
            req.InStockOnly,
            req.ActiveOnly,
            req.PageNumber,
            req.PageSize,
            req.SortBy,
            req.SortDescending);

        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<UniversalSearchResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var searchResult = result.Value;
        var response = new UniversalSearchResponseV1
        {
            Items = searchResult.Items,
            Facets = searchResult.Facets,
            TotalCount = searchResult.TotalCount,
            PageNumber = searchResult.PageNumber,
            PageSize = searchResult.PageSize,
            TotalPages = searchResult.TotalPages,
            HasPreviousPage = searchResult.HasPreviousPage,
            HasNextPage = searchResult.HasNextPage,
            Query = searchResult.Query
        };

        var apiResponse = ApiResponse<UniversalSearchResponseV1>.Success(
            response,
            "Search completed successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }

    private Dictionary<string, string[]> ParseAttributeFiltersFromQuery()
    {
        var attributeFilters = new Dictionary<string, string[]>();
        
        try
        {
            // Parse query string manually to handle complex nested parameters like attributeFilters[brand][0]=Dell
            foreach (var kvp in HttpContext.Request.Query)
            {
                var key = kvp.Key;
                var values = kvp.Value.ToArray();

                // Look for attributeFilters[attributeName][index] pattern
                if (key.StartsWith("attributeFilters[") && key.Contains("][") && key.EndsWith("]"))
                {
                    // Extract attribute name from attributeFilters[attributeName][index]
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
            // If parsing fails, return empty dictionary to avoid breaking the search
            return new Dictionary<string, string[]>();
        }

        return attributeFilters;
    }
}