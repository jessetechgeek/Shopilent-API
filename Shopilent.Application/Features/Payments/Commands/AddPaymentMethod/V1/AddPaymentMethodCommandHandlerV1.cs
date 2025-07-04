using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Identity.Repositories.Write;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Errors;
using Shopilent.Domain.Payments.Repositories.Write;
using Shopilent.Domain.Payments.ValueObjects;

namespace Shopilent.Application.Features.Payments.Commands.AddPaymentMethod.V1;

internal sealed class
    AddPaymentMethodCommandHandlerV1 : ICommandHandler<AddPaymentMethodCommandV1, AddPaymentMethodResponseV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<AddPaymentMethodCommandHandlerV1> _logger;

    public AddPaymentMethodCommandHandlerV1(
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        ILogger<AddPaymentMethodCommandHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<Result<AddPaymentMethodResponseV1>> Handle(AddPaymentMethodCommandV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get current user
            var userId = _currentUserContext.UserId;
            if (!userId.HasValue)
            {
                _logger.LogWarning("User not authenticated");
                return Result.Failure<AddPaymentMethodResponseV1>(Error.Unauthorized(
                    code: "User.NotAuthenticated",
                    message: "User must be authenticated to add payment methods"));
            }

            var user = await _unitOfWork.UserWriter.GetByIdAsync(userId.Value, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return Result.Failure<AddPaymentMethodResponseV1>(UserErrors.NotFound(userId.Value));
            }

            // Check if token already exists for this user
            var existingPaymentMethod =
                await _unitOfWork.PaymentMethodWriter.GetByTokenAsync(request.Token, cancellationToken);
            if (existingPaymentMethod != null && existingPaymentMethod.UserId == userId)
            {
                _logger.LogWarning("Payment method with token already exists for user: {UserId}", userId);
                return Result.Failure<AddPaymentMethodResponseV1>(PaymentMethodErrors.DuplicateTokenForUser);
            }

            // Parse enums
            if (!Enum.TryParse<PaymentMethodType>(request.Type, true, out var methodType))
            {
                return Result.Failure<AddPaymentMethodResponseV1>(PaymentMethodErrors.InvalidProviderType);
            }

            if (!Enum.TryParse<PaymentProvider>(request.Provider, true, out var provider))
            {
                return Result.Failure<AddPaymentMethodResponseV1>(PaymentMethodErrors.InvalidProviderType);
            }

            // Create payment method based on type
            Result<PaymentMethod> paymentMethodResult = methodType switch
            {
                PaymentMethodType.CreditCard => CreateCreditCardPaymentMethod(user, request, provider),
                PaymentMethodType.PayPal => CreatePayPalPaymentMethod(user, request, provider),
                _ => Result.Failure<PaymentMethod>(PaymentMethodErrors.InvalidProviderType)
            };

            if (paymentMethodResult.IsFailure)
            {
                _logger.LogWarning("Failed to create payment method: {Error}", paymentMethodResult.Error);
                return Result.Failure<AddPaymentMethodResponseV1>(paymentMethodResult.Error);
            }

            var paymentMethod = paymentMethodResult.Value;

            // Add any additional metadata
            foreach (var metadata in request.Metadata ?? new Dictionary<string, object>())
            {
                paymentMethod.UpdateMetadata(metadata.Key, metadata.Value);
            }

            // Add to repository and save using Unit of Work
            await _unitOfWork.PaymentMethodWriter.AddAsync(paymentMethod, cancellationToken);
            await _unitOfWork.SaveEntitiesAsync(cancellationToken);

            _logger.LogInformation("Payment method created successfully: {PaymentMethodId} for user: {UserId}",
                paymentMethod.Id, userId);

            // Map to response
            var response = new AddPaymentMethodResponseV1
            {
                Id = paymentMethod.Id,
                Type = paymentMethod.Type.ToString(),
                Provider = paymentMethod.Provider.ToString(),
                DisplayName = paymentMethod.DisplayName,
                CardBrand = paymentMethod.CardBrand,
                LastFourDigits = paymentMethod.LastFourDigits,
                ExpiryDate = paymentMethod.ExpiryDate,
                IsDefault = paymentMethod.IsDefault,
                IsActive = paymentMethod.IsActive,
                Metadata = paymentMethod.Metadata,
                CreatedAt = paymentMethod.CreatedAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method for user: {UserId}", _currentUserContext.UserId);
            return Result.Failure<AddPaymentMethodResponseV1>(
                Error.Failure(
                    code: "PaymentMethod.CreationFailed",
                    message: $"Failed to create payment method: {ex.Message}"));
        }
    }

    private static Result<PaymentMethod> CreateCreditCardPaymentMethod(
        Domain.Identity.User user,
        AddPaymentMethodCommandV1 request,
        PaymentProvider provider)
    {
        // Create card details value object
        var cardDetailsResult = PaymentCardDetails.Create(
            request.CardBrand,
            request.LastFourDigits,
            request.ExpiryDate ?? DateTime.UtcNow.AddYears(1));

        if (cardDetailsResult.IsFailure)
        {
            return Result.Failure<PaymentMethod>(cardDetailsResult.Error);
        }

        // Use the correct factory method
        return PaymentMethod.CreateCardMethod(
            user,
            provider,
            request.Token,
            cardDetailsResult.Value,
            request.IsDefault);
    }

    private static Result<PaymentMethod> CreatePayPalPaymentMethod(
        Domain.Identity.User user,
        AddPaymentMethodCommandV1 request,
        PaymentProvider provider)
    {
        return PaymentMethod.CreatePayPalMethod(
            user,
            request.Token,
            request.Email,
            request.IsDefault);
    }
}