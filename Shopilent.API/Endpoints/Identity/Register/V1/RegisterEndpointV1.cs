using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.Register.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Identity.Register.V1;

public class RegisterEndpointV1 : Endpoint<RegisterRequestV1, ApiResponse<RegisterResponseV1>>
{
    private readonly IMediator _mediator;

    public RegisterEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/auth/register");
        AllowAnonymous();
        Description(b => b
            .WithName("Register")
            .Produces<ApiResponse<RegisterResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<RegisterResponseV1>>(StatusCodes.Status400BadRequest)
            .WithTags("Identity"));
    }

    public override async Task HandleAsync(RegisterRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<RegisterResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }
        // Map the request to command
        var command = new RegisterCommandV1
        {
            Email = req.Email,
            Password = req.Password,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Phone = req.Phone
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var errorResponse = new ApiResponse<RegisterResponseV1>
            {
                Succeeded = false,
                Message = result.Error.Message,
                StatusCode = result.Error.Type == ErrorType.Validation
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status500InternalServerError,
                Errors = new[] { result.Error.Message }
            };

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Handle successful registration
        var response = new ApiResponse<RegisterResponseV1>
        {
            Succeeded = true,
            Message = "Registration successful",
            StatusCode = StatusCodes.Status201Created,
            Data = new RegisterResponseV1
            {
                Id = result.Value.User.Id,
                Email = result.Value.User.Email.Value,
                FirstName = result.Value.User.FullName.FirstName,
                LastName = result.Value.User.FullName.LastName,
                EmailVerified = result.Value.User.EmailVerified,
                Message = "Please check your email to verify your account",
                AccessToken = result.Value.AccessToken,
                RefreshToken = result.Value.RefreshToken
            }
        };

        await SendCreatedAtAsync("GetUserById", new { id = result.Value.User.Id }, response, cancellation: ct);
    }
}