using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Commands.UpdateUser.V1;
using Shopilent.Domain.Identity.DTOs;

namespace Shopilent.API.Endpoints.Users.UpdateUser.V1;

public class UpdateUserEndpointV1 : Endpoint<UpdateUserRequestV1, ApiResponse<UserDto>>
{
    private readonly ISender _sender;

    public UpdateUserEndpointV1(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Put("v1/users/{id}");
        Description(b => b
            .WithName("UpdateUser")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Users"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateUserRequestV1 req, CancellationToken ct)
    {
        // Get user ID from route
        var userId = Route<Guid>("id");

        // Get client info from context
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

        var command = new UpdateUserCommandV1
        {
            UserId = userId,
            FirstName = req.FirstName,
            LastName = req.LastName,
            MiddleName = req.MiddleName,
            Phone = req.Phone,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                Domain.Common.Errors.ErrorType.NotFound => StatusCodes.Status404NotFound,
                Domain.Common.Errors.ErrorType.Validation => StatusCodes.Status400BadRequest,
                Domain.Common.Errors.ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                Domain.Common.Errors.ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = new ApiResponse<UserDto>
            {
                Succeeded = false,
                Message = result.Error.Message,
                StatusCode = statusCode,
                Errors = new[] { result.Error.Message }
            };

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map domain User to DTO
        var userDto = new UserDto
        {
            Id = result.Value.User.Id,
            Email = result.Value.User.Email.Value,
            FirstName = result.Value.User.FullName.FirstName,
            LastName = result.Value.User.FullName.LastName,
            MiddleName = result.Value.User.FullName.MiddleName,
            Phone = result.Value.User.Phone?.Value,
            Role = result.Value.User.Role,
            IsActive = result.Value.User.IsActive,
            LastLogin = result.Value.User.LastLogin,
            EmailVerified = result.Value.User.EmailVerified,
            CreatedAt = result.Value.User.CreatedAt,
            UpdatedAt = result.Value.User.UpdatedAt
        };

        var response = new ApiResponse<UserDto>
        {
            Succeeded = true,
            Message = "User updated successfully",
            StatusCode = StatusCodes.Status200OK,
            Data = userDto
        };

        await SendAsync(response, response.StatusCode, ct);
    }
}