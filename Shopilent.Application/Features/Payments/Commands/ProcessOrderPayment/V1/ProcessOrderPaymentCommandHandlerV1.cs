using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.Services;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.DTOs;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Errors;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Errors;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Application.Features.Payments.Commands.ProcessOrderPayment.V1;

internal sealed class ProcessOrderPaymentCommandHandlerV1
    : ICommandHandler<ProcessOrderPaymentCommandV1, ProcessOrderPaymentResponseV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<ProcessOrderPaymentCommandHandlerV1> _logger;

    public ProcessOrderPaymentCommandHandlerV1(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ICurrentUserContext currentUserContext,
        ILogger<ProcessOrderPaymentCommandHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<Result<ProcessOrderPaymentResponseV1>> Handle(
        ProcessOrderPaymentCommandV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the order
            var order = await _unitOfWork.OrderWriter.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
                return Result.Failure<ProcessOrderPaymentResponseV1>(OrderErrors.NotFound(request.OrderId));
            }

            // Validate order status
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("Invalid order status for payment processing. OrderId: {OrderId}, Status: {Status}",
                    request.OrderId, order.Status);
                return Result.Failure<ProcessOrderPaymentResponseV1>(
                    OrderErrors.InvalidOrderStatus("payment processing"));
            }

            // Check if order already has successful payment
            if (order.PaymentStatus == PaymentStatus.Succeeded)
            {
                _logger.LogWarning("Order already paid. OrderId: {OrderId}", request.OrderId);
                return Result.Failure<ProcessOrderPaymentResponseV1>(
                    PaymentErrors.InvalidPaymentStatus("duplicate payment"));
            }

            // Validate user authorization
            if (_currentUserContext.UserId.HasValue && order.UserId != _currentUserContext.UserId.Value)
            {
                _logger.LogWarning("Unauthorized payment attempt. UserId: {UserId}, OrderUserId: {OrderUserId}",
                    _currentUserContext.UserId, order.UserId);
                return Result.Failure<ProcessOrderPaymentResponseV1>(
                    Error.Unauthorized("User.UnauthorizedAccess",
                        "You are not authorized to process payment for this order."));
            }

            // Get user for payment
            User user = null;
            if (order.UserId.HasValue)
            {
                user = await _unitOfWork.UserWriter.GetByIdAsync(order.UserId.Value, cancellationToken);
            }

            // Validate payment method if provided and get token
            PaymentMethodDto paymentMethod = null;
            if (request.PaymentMethodId.HasValue)
            {
                paymentMethod = await _unitOfWork.PaymentMethodReader.GetByIdAsync(
                    request.PaymentMethodId.Value, cancellationToken);

                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method not found. PaymentMethodId: {PaymentMethodId}",
                        request.PaymentMethodId);
                    return Result.Failure<ProcessOrderPaymentResponseV1>(
                        PaymentErrors.PaymentMethodNotFound(request.PaymentMethodId.Value));
                }

                if (user != null && paymentMethod.UserId != user.Id)
                {
                    _logger.LogWarning(
                        "Payment method belongs to different user. PaymentMethodId: {PaymentMethodId}, UserId: {UserId}",
                        request.PaymentMethodId, user.Id);
                    return Result.Failure<ProcessOrderPaymentResponseV1>(
                        Error.Unauthorized("PaymentMethod.UnauthorizedAccess",
                            "Payment method does not belong to the user."));
                }
            }

            // Create payment amount
            var orderAmount = Money.Create(order.Total.Amount, order.Total.Currency ?? "USD");
            if (orderAmount.IsFailure)
            {
                _logger.LogError("Invalid order amount. OrderId: {OrderId}, Amount: {Amount}",
                    request.OrderId, order.Total);
                return Result.Failure<ProcessOrderPaymentResponseV1>(orderAmount.Error);
            }

            // Determine which payment token to use
            var paymentToken = paymentMethod?.Token ?? request.PaymentMethodToken;

            // Extract customer ID based on provider
            string customerId = null;
            if (paymentMethod?.Metadata != null)
            {
                // Get customer ID for the specific provider
                var customerIdKey = request.Provider switch
                {
                    PaymentProvider.Stripe => "stripe_customer_id",
                    PaymentProvider.PayPal => "paypal_customer_id",
                    _ => null
                };

                if (customerIdKey != null && paymentMethod.Metadata.TryGetValue(customerIdKey, out var customerIdObj))
                {
                    customerId = customerIdObj?.ToString();
                }
            }

            // Process payment with external provider
            var paymentResult = await _paymentService.ProcessPaymentAsync(
                orderAmount.Value,
                request.MethodType,
                request.Provider,
                paymentToken,
                customerId,
                request.ExternalReference,
                request.Metadata,
                cancellationToken);

            if (paymentResult.IsFailure)
            {
                _logger.LogError("Payment processing failed. OrderId: {OrderId}, Error: {Error}",
                    request.OrderId, paymentResult.Error);

                // Create failed payment record
                var failedPayment = Payment.Create(
                    order,
                    user,
                    orderAmount.Value,
                    request.MethodType,
                    request.Provider,
                    request.ExternalReference);

                if (failedPayment.IsSuccess)
                {
                    var markFailedResult = failedPayment.Value.MarkAsFailed(paymentResult.Error.Message);
                    if (markFailedResult.IsSuccess)
                    {
                        await _unitOfWork.PaymentWriter.AddAsync(failedPayment.Value, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }

                return Result.Failure<ProcessOrderPaymentResponseV1>(paymentResult.Error);
            }

            var transactionId = paymentResult.Value;

            // Create successful payment record
            var payment = Payment.Create(
                order,
                user,
                orderAmount.Value,
                request.MethodType,
                request.Provider,
                request.ExternalReference);

            if (payment.IsFailure)
            {
                _logger.LogError("Failed to create payment record. OrderId: {OrderId}, Error: {Error}",
                    request.OrderId, payment.Error);
                return Result.Failure<ProcessOrderPaymentResponseV1>(payment.Error);
            }

            // Mark payment as succeeded
            var markSuccessResult = payment.Value.MarkAsSucceeded(transactionId);
            if (markSuccessResult.IsFailure)
            {
                _logger.LogError("Failed to mark payment as succeeded. OrderId: {OrderId}, Error: {Error}",
                    request.OrderId, markSuccessResult.Error);
                return Result.Failure<ProcessOrderPaymentResponseV1>(markSuccessResult.Error);
            }

            // Add metadata if provided
            if (request.Metadata?.Any() == true)
            {
                foreach (var kvp in request.Metadata)
                {
                    payment.Value.UpdateMetadata(kvp.Key, kvp.Value);
                }
            }

            // Update order payment status
            var updateOrderResult = order.MarkAsPaid();
            if (updateOrderResult.IsFailure)
            {
                _logger.LogError("Failed to mark order as paid. OrderId: {OrderId}, Error: {Error}",
                    request.OrderId, updateOrderResult.Error);
                return Result.Failure<ProcessOrderPaymentResponseV1>(updateOrderResult.Error);
            }

            // Save changes
            await _unitOfWork.PaymentWriter.AddAsync(payment.Value, cancellationToken);
            await _unitOfWork.OrderWriter.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Payment processed successfully. OrderId: {OrderId}, PaymentId: {PaymentId}, TransactionId: {TransactionId}",
                request.OrderId, payment.Value.Id, transactionId);

            return Result.Success(new ProcessOrderPaymentResponseV1
            {
                PaymentId = payment.Value.Id,
                OrderId = order.Id,
                Amount = payment.Value.Amount.Amount,
                Currency = payment.Value.Amount.Currency,
                Status = payment.Value.Status,
                MethodType = payment.Value.MethodType,
                Provider = payment.Value.Provider,
                TransactionId = payment.Value.TransactionId,
                ExternalReference = payment.Value.ExternalReference,
                ProcessedAt = payment.Value.ProcessedAt ?? DateTime.UtcNow,
                Message = "Payment processed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order. OrderId: {OrderId}", request.OrderId);

            return Result.Failure<ProcessOrderPaymentResponseV1>(
                Error.Failure(
                    code: "Payment.ProcessingError",
                    message: $"An error occurred while processing the payment: {ex.Message}"));
        }
    }
}