using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;

namespace Shopilent.Application.Features.Identity.Commands.UpdateUserStatus.V1;

internal sealed class UpdateUserStatusCommandHandlerV1 : ICommandHandler<UpdateUserStatusCommandV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<UpdateUserStatusCommandHandlerV1> _logger;

    public UpdateUserStatusCommandHandlerV1(
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        ILogger<UpdateUserStatusCommandHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUserStatusCommandV1 request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user by ID
            var user = await _unitOfWork.UserWriter.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
            {
                return Result.Failure(UserErrors.NotFound(request.Id));
            }

            // Prevent self-deactivation
            if (_currentUserContext.UserId.HasValue && 
                _currentUserContext.UserId.Value == request.Id && 
                !request.IsActive)
            {
                return Result.Failure(UserErrors.CannotDeactivateSelf);
            }

            // Update status
            Result statusResult;
            if (request.IsActive)
            {
                statusResult = user.Activate();
            }
            else
            {
                statusResult = user.Deactivate();
            }

            if (statusResult.IsFailure)
            {
                return statusResult;
            }

            // Set audit info if user context is available
            if (_currentUserContext.UserId.HasValue)
            {
                user.SetAuditInfo(_currentUserContext.UserId);
            }

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User status updated successfully. ID: {UserId}, IsActive: {IsActive}", 
                user.Id, request.IsActive);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status. ID: {UserId}, IsActive: {IsActive}", 
                request.Id, request.IsActive);

            return Result.Failure(
                Error.Failure(
                    code: "User.UpdateStatusFailed",
                    message: $"Failed to update user status: {ex.Message}"));
        }
    }
}