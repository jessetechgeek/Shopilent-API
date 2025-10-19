using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.CreateCart.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.CreateCart.V1;

public class CreateCartEndpointV1 : Endpoint<CreateCartRequestV1, ApiResponse<CreateCartResponseV1>>
{
    private readonly IMediator _mediator;

    public CreateCartEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/cart");
        Description(b => b
            .WithName("CreateCart")
            .Produces<ApiResponse<CreateCartResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateCartResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateCartResponseV1>>(StatusCodes.Status401Unauthorized)
            .WithTags("Shopping Cart"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CreateCartRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<CreateCartResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Create command
        var command = new CreateCartCommandV1
        {
            Metadata = req.Metadata
        };

        // Send command to mediator
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<CreateCartResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<CreateCartResponseV1>.Success(
            result.Value,
            "Cart created successfully");

        await SendAsync(response, StatusCodes.Status201Created, ct);
    }
}