using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Queries.GetOrdersDatatable.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.Endpoints.Sales.GetOrdersDatatable.V1;

public class GetOrdersDatatableEndpointV1 : Endpoint<DataTableRequest, ApiResponse<DataTableResult<OrderDatatableDto>>>
{
    private readonly IMediator _mediator;

    public GetOrdersDatatableEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/orders/datatable");
        Description(b => b
            .WithName("GetOrdersDatatable")
            .Produces<ApiResponse<DataTableResult<OrderDatatableDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DataTableResult<OrderDatatableDto>>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DataTableResult<OrderDatatableDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<DataTableResult<OrderDatatableDto>>>(StatusCodes.Status403Forbidden)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(DataTableRequest req, CancellationToken ct)
    {
        // Create query
        var query = new GetOrdersDatatableQueryV1 { Request = req };

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

            var errorResponse = ApiResponse<DataTableResult<OrderDatatableDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<DataTableResult<OrderDatatableDto>>.Success(
            result.Value,
            "Orders retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}