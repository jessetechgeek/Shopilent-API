// API/Endpoints/Catalog/GetAttributesDatatable/V1/GetAttributesDatatableEndpointV1.cs

using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Queries.GetAttributesDatatable.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.Endpoints.Catalog.Attributes.GetAttributesDatatable.V1;

public class GetAttributesDatatableEndpointV1 : Endpoint<DataTableRequest, ApiResponse<DataTableResult<AttributeDatatableDto>>>
{
    private readonly IMediator _mediator;

    public GetAttributesDatatableEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/attributes/datatable");
        Description(b => b
            .WithName("GetAttributesDatatable")
            .Produces<ApiResponse<DataTableResult<AttributeDatatableDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DataTableResult<AttributeDatatableDto>>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DataTableResult<AttributeDatatableDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<DataTableResult<AttributeDatatableDto>>>(StatusCodes.Status403Forbidden)
            .WithTags("Attributes"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(DataTableRequest req, CancellationToken ct)
    {
        // Create query
        var query = new GetAttributesDatatableQueryV1 { Request = req };

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

            var errorResponse = ApiResponse<DataTableResult<AttributeDatatableDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<DataTableResult<AttributeDatatableDto>>.Success(
            result.Value,
            "Attributes retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}