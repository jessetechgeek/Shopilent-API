using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1;
using Shopilent.Domain.Common.Errors;
using ProductAttributeDto = Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1.ProductAttributeDto;

namespace Shopilent.API.Endpoints.Catalog.Products.UpdateProduct.V1;

public class UpdateProductEndpointV1 : Endpoint<UpdateProductRequestV1, ApiResponse<UpdateProductResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateProductEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/products/{id}");
        AllowFileUploads();
        Description(b => b
            .WithName("UpdateProduct")
            .Produces<ApiResponse<UpdateProductResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateProductResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateProductResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateProductResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateProductResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Products"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateProductRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateProductResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // var command = new UpdateProductCommandV1
        // {
        //     Id = id,
        //     Name = req.Name,
        //     Description = req.Description,
        //     BasePrice = req.BasePrice,
        //     Slug = req.Slug,
        //     Sku = req.Sku,
        //     IsActive = req.IsActive,
        //     CategoryIds = req.CategoryIds,
        //     Attributes = req.Attributes?.Select(a => new ProductAttributeDto
        //     {
        //         AttributeId = a.AttributeId,
        //         Value = a.Value
        //     }).ToList(),
        //     Images = req.File?.Select(i => new ProductImageDto
        //     {
        //         Url = i.OpenReadStream(),
        //         AltText = "Updated image",
        //         IsDefault = false,
        //         DisplayOrder = 0
        //     }).ToList(),
        //     RemoveExistingImages = req.RemoveExistingImages,
        //     ImagesToRemove = req.ImagesToRemove // Add the new property
        // };
        var command = new UpdateProductCommandV1
        {
            Id = id,
            Name = req.Name,
            Description = req.Description,
            BasePrice = req.BasePrice,
            Slug = req.Slug,
            Sku = req.Sku,
            IsActive = req.IsActive,
            CategoryIds = req.CategoryIds,
            Attributes = req.Attributes?.Select(a => new ProductAttributeDto
            {
                AttributeId = a.AttributeId,
                Value = a.Value
            }).ToList(),
            Images = req.File?.Select(i => new ProductImageDto
            {
                Url = i.OpenReadStream(),
                AltText = "Updated image",
                IsDefault = false,
                DisplayOrder = 0
            }).ToList(),
            RemoveExistingImages = req.RemoveExistingImages,
            ImagesToRemove = req.ImagesToRemove,
    
            // Add the new mapping
            ImageOrders = req.ImageOrders?.Select(io => new ProductImageOrderDto
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

            var errorResponse = ApiResponse<UpdateProductResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var apiResponse = ApiResponse<UpdateProductResponseV1>.Success(
            result.Value,
            "Product updated successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}