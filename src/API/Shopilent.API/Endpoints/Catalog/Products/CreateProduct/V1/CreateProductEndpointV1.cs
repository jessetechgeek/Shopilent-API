using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;

public class CreateProductEndpointV1 : Endpoint<CreateProductRequestV1, ApiResponse<CreateProductResponseV1>>
{
    private readonly IMediator _mediator;

    public CreateProductEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/products");
        AllowFileUploads();
        Description(b => b
            .WithName("CreateProduct")
            .Produces<ApiResponse<CreateProductResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateProductResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateProductResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<CreateProductResponseV1>>(StatusCodes.Status403Forbidden)
            .WithTags("Products"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(CreateProductRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<CreateProductResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new CreateProductCommandV1
        {
            Name = req.Name,
            Slug = req.Slug,
            Description = req.Description,
            BasePrice = req.BasePrice,
            Currency = req.Currency,
            Sku = req.Sku,
            CategoryIds = req.CategoryIds,
            Metadata = req.Metadata,
            IsActive = req.IsActive,
            Attributes = req.Attributes?.Select(a => new ProductAttributeDto
            {
                AttributeId = a.AttributeId,
                Value = a.Value
            }).ToList() ?? new List<ProductAttributeDto>(),
            Images = req.File?.Select(i => new ProductImageDto
            {
                Url = i.OpenReadStream(),
                AltText = "sample text",
                IsDefault = true,
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

            var errorResponse = ApiResponse<CreateProductResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map command result to response
        var response = new CreateProductResponseV1
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Slug = result.Value.Slug,
            Description = result.Value.Description,
            BasePrice = result.Value.BasePrice,
            Currency = result.Value.Currency,
            Sku = result.Value.Sku,
            IsActive = result.Value.IsActive,
            Metadata = result.Value.Metadata,
            CategoryIds = result.Value.CategoryIds,
            Images = result.Value.Images,
            CreatedAt = result.Value.CreatedAt
        };

        // Return successful response
        var apiResponse = ApiResponse<CreateProductResponseV1>.Success(
            response,
            "Product created successfully");

        await SendCreatedAtAsync("GetProductById", new { id = response.Id }, apiResponse, cancellation: ct);
    }
}