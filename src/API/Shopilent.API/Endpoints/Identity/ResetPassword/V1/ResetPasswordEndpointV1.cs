using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.ResetPassword.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Identity.ResetPassword.V1;

public class ResetPasswordEndpointV1 : Endpoint<ResetPasswordRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public ResetPasswordEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/auth/reset-password");
        AllowAnonymous();
        Description(b => b
            .WithName("ResetPassword")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .WithTags("Identity"));
    }

    public override async Task HandleAsync(ResetPasswordRequestV1 req, CancellationToken ct)
    {
        // Validate request early
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map the request to command
        var command = new ResetPasswordCommandV1
        {
            Token = req.Token,
            NewPassword = req.NewPassword,
            ConfirmPassword = req.ConfirmPassword
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
            "Password has been reset successfully. You can now log in with your new password.",
            "Password reset successful");

        await SendAsync(response, response.StatusCode, ct);
    }
}