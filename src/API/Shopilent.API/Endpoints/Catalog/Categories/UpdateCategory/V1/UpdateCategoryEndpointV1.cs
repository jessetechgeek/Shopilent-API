using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.UpdateCategory.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.UpdateCategory.V1;

public class UpdateCategoryEndpointV1 : Endpoint<UpdateCategoryRequestV1, ApiResponse<UpdateCategoryResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateCategoryEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/categories/{id}");
        Description(b => b
            .WithName("UpdateCategory")
            .Produces<ApiResponse<UpdateCategoryResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateCategoryResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateCategoryResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateCategoryResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateCategoryResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Categories"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateCategoryRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateCategoryResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new UpdateCategoryCommandV1
        {
            Id = id,
            Name = req.Name,
            Slug = req.Slug,
            Description = req.Description,
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
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<UpdateCategoryResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map command result to response
        var response = new UpdateCategoryResponseV1
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Slug = result.Value.Slug,
            Description = result.Value.Description,
            ParentId = result.Value.ParentId,
            Level = result.Value.Level,
            Path = result.Value.Path,
            IsActive = result.Value.IsActive,
            UpdatedAt = result.Value.UpdatedAt
        };

        // Return successful response
        var apiResponse = ApiResponse<UpdateCategoryResponseV1>.Success(
            response,
            "Category updated successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}