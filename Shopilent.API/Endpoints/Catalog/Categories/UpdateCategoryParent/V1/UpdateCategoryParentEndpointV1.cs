using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.UpdateCategoryParent.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.UpdateCategoryParent.V1;

public class UpdateCategoryParentEndpointV1 : Endpoint<UpdateCategoryParentRequestV1, ApiResponse<UpdateCategoryParentResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateCategoryParentEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/categories/{id}/parent");
        Description(b => b
            .WithName("UpdateCategoryParent")
            .Produces<ApiResponse<UpdateCategoryParentResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateCategoryParentResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateCategoryParentResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateCategoryParentResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateCategoryParentResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Categories"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateCategoryParentRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        // Map request to command
        var command = new UpdateCategoryParentCommandV1
        {
            Id = id,
            ParentId = req.ParentId
        };

        // Send command to mediator
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<UpdateCategoryParentResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map command result to response
        var response = new UpdateCategoryParentResponseV1
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Slug = result.Value.Slug,
            ParentId = result.Value.ParentId,
            ParentName = result.Value.ParentName,
            Level = result.Value.Level,
            Path = result.Value.Path,
            UpdatedAt = result.Value.UpdatedAt
        };

        // Return successful response
        var apiResponse = ApiResponse<UpdateCategoryParentResponseV1>.Success(
            response,
            "Category parent updated successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}