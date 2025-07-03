using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Queries.GetCategoriesDatatable.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetCategoriesDatatable.V1;

public class
    GetCategoriesDatatableEndpointV1 : Endpoint<DataTableRequest, ApiResponse<DataTableResult<CategoryDatatableDto>>>
{
    private readonly IMediator _mediator;

    public GetCategoriesDatatableEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/categories/datatable");
        Description(b => b
            .WithName("GetCategoriesDatatable")
            .Produces<ApiResponse<DataTableResult<CategoryDatatableDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DataTableResult<CategoryDatatableDto>>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DataTableResult<CategoryDatatableDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<DataTableResult<CategoryDatatableDto>>>(StatusCodes.Status403Forbidden)
            .WithTags("Categories"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(DataTableRequest req, CancellationToken ct)
    {
        // Create query
        var query = new GetCategoriesDatatableQueryV1 { Request = req };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<DataTableResult<CategoryDatatableDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<DataTableResult<CategoryDatatableDto>>.Success(
            result.Value,
            "Categories retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}