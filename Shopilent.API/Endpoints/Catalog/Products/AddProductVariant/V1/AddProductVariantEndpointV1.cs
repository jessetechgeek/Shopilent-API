using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.AddProductVariant.V1;

public class AddProductVariantEndpointV1 : Endpoint<AddProductVariantRequestV1, ApiResponse<AddProductVariantResponseV1>>
{
    private readonly IMediator _mediator;

    public AddProductVariantEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/products/{id}/variants");
        AllowFileUploads();
        Description(b => b
            .WithName("AddProductVariant")
            .Produces<ApiResponse<AddProductVariantResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<AddProductVariantResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<AddProductVariantResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<AddProductVariantResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<AddProductVariantResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Products"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(AddProductVariantRequestV1 req, CancellationToken ct)
    {
        // Get product ID from route
        var productId = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<AddProductVariantResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new AddProductVariantCommandV1
        {
            ProductId = productId,
            Sku = req.Sku,
            Price = req.Price,
            StockQuantity = req.StockQuantity,
            Attributes = req.Attributes,
            IsActive = req.IsActive,
            Metadata = req.Metadata,
            Images = req.File?.Select(i => new ProductImageDto
            {
                Url = i.OpenReadStream(),
                AltText = "sample text",
                IsDefault = false,
                DisplayOrder = 0
            }).ToList() ?? new List<ProductImageDto>()
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

            var errorResponse = ApiResponse<AddProductVariantResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var apiResponse = ApiResponse<AddProductVariantResponseV1>.Success(
            result.Value,
            "Product variant created successfully");

        await SendCreatedAtAsync("GetVariantById", new { id = result.Value.Id }, apiResponse, cancellation: ct);
    }
}