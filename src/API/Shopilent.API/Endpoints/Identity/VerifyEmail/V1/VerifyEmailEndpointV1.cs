using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.VerifyEmail.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Identity.VerifyEmail.V1;

public class VerifyEmailEndpointV1 : Endpoint<VerifyEmailRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public VerifyEmailEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/auth/verify-email/{token}");
        AllowAnonymous();
        Description(b => b
            .WithName("VerifyEmail")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .WithTags("Identity"));
    }

    public override async Task HandleAsync(VerifyEmailRequestV1 req, CancellationToken ct)
    {
        // Map the request to command
        var command = new VerifyEmailCommandV1
        {
            Token = req.Token
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<string>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<string>.Success(
            "Email verification successful. You can now log in.",
            "Email verification successful");

        await SendAsync(response, response.StatusCode, ct);
    }
}