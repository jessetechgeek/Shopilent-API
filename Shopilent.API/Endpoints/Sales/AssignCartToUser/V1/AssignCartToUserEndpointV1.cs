using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Sales.Commands.AssignCartToUser.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.AssignCartToUser.V1;

public class AssignCartToUserEndpointV1 : Endpoint<AssignCartToUserRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public AssignCartToUserEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/cart/assign");
        Description(b => b
            .WithName("AssignCartToUser")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Shopping Cart"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(AssignCartToUserRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map the request to command
        var command = new AssignCartToUserCommandV1
        {
            CartId = req.CartId
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
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

        var response = ApiResponse<string>.Success(
            "Cart successfully assigned to user",
            "Cart assigned to user");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}