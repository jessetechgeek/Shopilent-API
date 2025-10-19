using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.UpdateVariantStock.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.UpdateVariantStock.V1;

public class UpdateVariantStockEndpointV1 : Endpoint<UpdateVariantStockRequestV1, ApiResponse<UpdateVariantStockResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateVariantStockEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/variants/{id}/stock");
        Description(b => b
            .WithName("UpdateVariantStock")
            .Produces<ApiResponse<UpdateVariantStockResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateVariantStockResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateVariantStockResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateVariantStockResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateVariantStockResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Variants"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateVariantStockRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateVariantStockResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new UpdateVariantStockCommandV1
        {
            Id = id,
            StockQuantity = req.Quantity
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

            var errorResponse = ApiResponse<UpdateVariantStockResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map command result to response
        var response = new UpdateVariantStockResponseV1
        {
            Id = result.Value.Id,
            StockQuantity = result.Value.StockQuantity,
            IsActive = result.Value.IsActive,
            UpdatedAt = result.Value.UpdatedAt
        };

        // Return successful response
        var apiResponse = ApiResponse<UpdateVariantStockResponseV1>.Success(
            response,
            "Variant stock updated successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}