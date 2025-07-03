using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.DTOs;
using Shopilent.Domain.Identity.Errors;

namespace Shopilent.Application.Features.Identity.Queries.GetCurrentUserProfile.V1;

internal sealed class GetCurrentUserProfileQueryHandlerV1 : IQueryHandler<GetCurrentUserProfileQueryV1, UserDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCurrentUserProfileQueryHandlerV1> _logger;

    public GetCurrentUserProfileQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetCurrentUserProfileQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDetailDto>> Handle(
        GetCurrentUserProfileQueryV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userProfile = await _unitOfWork.UserReader.GetDetailByIdAsync(request.UserId, cancellationToken);

            if (userProfile == null)
            {
                _logger.LogWarning("User profile not found for user ID: {UserId}", request.UserId);
                return Result.Failure<UserDetailDto>(UserErrors.NotFound(request.UserId));
            }

            _logger.LogInformation("Retrieved user profile for user ID: {UserId}", request.UserId);
            return Result.Success(userProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for user ID: {UserId}", request.UserId);

            return Result.Failure<UserDetailDto>(
                Error.Failure(
                    code: "UserProfile.GetFailed",
                    message: $"Failed to retrieve user profile: {ex.Message}"));
        }
    }
}