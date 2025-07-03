using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.ForgotPassword.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Identity.ForgotPassword.V1;

public class ForgotPasswordEndpointV1 : Endpoint<ForgotPasswordRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public ForgotPasswordEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/auth/forgot-password");
        AllowAnonymous();
        Description(b => b
            .WithName("ForgotPassword")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .WithTags("Identity"));
    }

    public override async Task HandleAsync(ForgotPasswordRequestV1 req, CancellationToken ct)
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
        var command = new ForgotPasswordCommandV1
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

        // For security reasons, always return success even if email doesn't exist
        var response = ApiResponse<string>.Success(
            "If the email exists, a password reset link has been sent to it.",
            "Password reset email sent");

        await SendAsync(response, response.StatusCode, ct);
    }
}