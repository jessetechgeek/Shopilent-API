using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.UpdateCategoryStatus.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.UpdateCategoryStatus.V1;

public class UpdateCategoryStatusEndpointV1 : Endpoint<UpdateCategoryStatusRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public UpdateCategoryStatusEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/categories/{id}/status");
        Description(b => b
            .WithName("UpdateCategoryStatus")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Categories"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateCategoryStatusRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        // Create command
        var command = new UpdateCategoryStatusCommandV1
        {
            Id = id,
            IsActive = req.IsActive
        };

        // Send command to mediator
        var result = await _mediator.Send(command, ct);

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

            var errorResponse = ApiResponse<string>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<string>.Success(
            $"Category status updated to {(req.IsActive ? "active" : "inactive")}",
            "Category status updated successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}