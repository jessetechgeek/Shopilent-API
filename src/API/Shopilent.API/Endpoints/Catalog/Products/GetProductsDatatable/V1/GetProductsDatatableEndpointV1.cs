using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Queries.GetProductsDatatable.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.Endpoints.Catalog.Products.GetProductsDatatable.V1;

public class
    GetProductsDatatableEndpointV1 : Endpoint<DataTableRequest, ApiResponse<DataTableResult<ProductDatatableDto>>>
{
    private readonly IMediator _mediator;

    public GetProductsDatatableEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/products/datatable");
        Description(b => b
            .WithName("GetProductsDatatable")
            .Produces<ApiResponse<DataTableResult<ProductDatatableDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DataTableResult<ProductDatatableDto>>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DataTableResult<ProductDatatableDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<DataTableResult<ProductDatatableDto>>>(StatusCodes.Status403Forbidden)
            .WithTags("Products"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(DataTableRequest req, CancellationToken ct)
    {
        // Create query
        var query = new GetProductsDatatableQueryV1 { Request = req };

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

            var errorResponse = ApiResponse<DataTableResult<ProductDatatableDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<DataTableResult<ProductDatatableDto>>.Success(
            result.Value,
            "Products retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}