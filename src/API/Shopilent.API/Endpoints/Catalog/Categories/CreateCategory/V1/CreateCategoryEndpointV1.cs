using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.CreateCategory.V1;

public class CreateCategoryEndpointV1 : Endpoint<CreateCategoryRequestV1, ApiResponse<CreateCategoryResponseV1>>
{
    private readonly IMediator _mediator;

    public CreateCategoryEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/categories");
        Description(b => b
            .WithName("CreateCategory")
            .Produces<ApiResponse<CreateCategoryResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateCategoryResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateCategoryResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<CreateCategoryResponseV1>>(StatusCodes.Status403Forbidden)
            .WithTags("Categories"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(CreateCategoryRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<CreateCategoryResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new CreateCategoryCommandV1
        {
            Name = req.Name,
            Slug = req.Slug,
            Description = req.Description,
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

            var errorResponse = ApiResponse<CreateCategoryResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map command result to response
        var response = new CreateCategoryResponseV1
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Slug = result.Value.Slug,
            Description = result.Value.Description,
            ParentId = result.Value.ParentId,
            Level = result.Value.Level,
            Path = result.Value.Path,
            IsActive = result.Value.IsActive,
            CreatedAt = result.Value.CreatedAt
        };

        // Return successful response
        var apiResponse = ApiResponse<CreateCategoryResponseV1>.Success(
            response,
            "Category created successfully");

        await SendCreatedAtAsync("GetCategoryById", new { id = response.Id }, apiResponse, cancellation: ct);
    }
}