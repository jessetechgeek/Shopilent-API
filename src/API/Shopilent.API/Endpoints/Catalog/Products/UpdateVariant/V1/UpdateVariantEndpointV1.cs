using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.UpdateVariant.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.UpdateVariant.V1;

public class UpdateVariantEndpointV1 : Endpoint<UpdateVariantRequestV1, ApiResponse<UpdateVariantResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateVariantEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/variants/{id}");
        AllowFileUploads();
        Description(b => b
            .WithName("UpdateVariant")
            .Produces<ApiResponse<UpdateVariantResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateVariantResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateVariantResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateVariantResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateVariantResponseV1>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<UpdateVariantResponseV1>>(StatusCodes.Status409Conflict)
            .WithTags("Variants"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateVariantRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateVariantResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new UpdateVariantCommandV1
        {
            Id = id,
            Sku = req.Sku,
            Price = req.Price,
            StockQuantity = req.StockQuantity,
            Metadata = req.Metadata,
            IsActive = req.IsActive,
            Images = req.File?.Select(i => new VariantImageDto
            {
                Url = i.OpenReadStream(),
                AltText = "Variant image",
                IsDefault = false,
                DisplayOrder = 0
            }).ToList(),
            RemoveExistingImages = req.RemoveExistingImages,
            ImagesToRemove = req.ImagesToRemove,
            ImageOrders = req.ImageOrders?.Select(io => new VariantImageOrderDto
            {
                ImageKey = io.ImageKey,
                DisplayOrder = io.DisplayOrder,
                IsDefault = io.IsDefault
            }).ToList()
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

            var errorResponse = ApiResponse<UpdateVariantResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var apiResponse = ApiResponse<UpdateVariantResponseV1>.Success(
            result.Value,
            "Product variant updated successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}