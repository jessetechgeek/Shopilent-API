using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Queries.GetUsersDatatable.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.Endpoints.Users.GetUsersDatatable.V1;

public class GetUsersDatatableEndpointV1 : Endpoint<DataTableRequest, ApiResponse<DataTableResult<UserDatatableDto>>>
{
    private readonly IMediator _mediator;

    public GetUsersDatatableEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/users/datatable");
        Description(b => b
            .WithName("GetUsersDatatable")
            .Produces<ApiResponse<DataTableResult<UserDatatableDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DataTableResult<UserDatatableDto>>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DataTableResult<UserDatatableDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<DataTableResult<UserDatatableDto>>>(StatusCodes.Status403Forbidden)
            .WithTags("Users"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(DataTableRequest req, CancellationToken ct)
    {
        // Create query
        var query = new GetUsersDatatableQueryV1 { Request = req };

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

            var errorResponse = ApiResponse<DataTableResult<UserDatatableDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<DataTableResult<UserDatatableDto>>.Success(
            result.Value,
            "Users retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}