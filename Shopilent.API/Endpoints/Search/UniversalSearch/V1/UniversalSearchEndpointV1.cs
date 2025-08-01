using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.API.Common.Services;
using Shopilent.Application.Features.Search.Queries.UniversalSearch.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Search.UniversalSearch.V1;

public class UniversalSearchEndpointV1 : Endpoint<UniversalSearchRequestV1, ApiResponse<UniversalSearchResponseV1>>
{
    private readonly ISender _sender;
    private readonly IFilterEncodingService _filterEncodingService;

    public UniversalSearchEndpointV1(ISender sender, IFilterEncodingService filterEncodingService)
    {
        _sender = sender;
        _filterEncodingService = filterEncodingService;
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

        var filtersResult = _filterEncodingService.DecodeFilters(req.FiltersBase64);
        if (filtersResult.IsFailure)
        {
            var errorResponse = ApiResponse<UniversalSearchResponseV1>.Failure(
                filtersResult.Error.Message,
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var filters = filtersResult.Value;

        var query = new UniversalSearchQueryV1(
            filters.SearchQuery,
            filters.CategoryIds,
            filters.AttributeFilters,
            filters.PriceMin,
            filters.PriceMax,
            filters.InStockOnly,
            filters.ActiveOnly,
            filters.PageNumber,
            filters.PageSize,
            filters.SortBy,
            filters.SortDescending);

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

}