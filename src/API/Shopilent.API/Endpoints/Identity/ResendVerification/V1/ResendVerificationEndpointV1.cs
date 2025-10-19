using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.ResendVerification.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Identity.ResendVerification.V1;

public class ResendVerificationEndpointV1 : Endpoint<ResendVerificationRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public ResendVerificationEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/auth/resend-verification");
        AllowAnonymous();
        Description(b => b
            .WithName("ResendVerification")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .WithTags("Identity"));
    }

    public override async Task HandleAsync(ResendVerificationRequestV1 req, CancellationToken ct)
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
        var command = new ResendVerificationCommandV1
        {
            Email = req.Email
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
            "Verification email sent successfully. Please check your inbox.",
            "Verification email sent");

        await SendAsync(response, response.StatusCode, ct);
    }
}