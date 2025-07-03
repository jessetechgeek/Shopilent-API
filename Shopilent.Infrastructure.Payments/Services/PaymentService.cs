using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Services;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Infrastructure.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly Dictionary<PaymentProvider, IPaymentProvider> _providers;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IEnumerable<IPaymentProvider> providers,
        ILogger<PaymentService> logger)
    {
        _providers = providers.ToDictionary(p => p.Provider, p => p);
        _logger = logger;
    }

    public async Task<Result<string>> ProcessPaymentAsync(
        Money amount,
        PaymentMethodType methodType,
        PaymentProvider provider,
        string paymentMethodToken,
        string externalReference = null,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_providers.TryGetValue(provider, out var paymentProvider))
            {
                _logger.LogError("Payment provider not configured: {Provider}", provider);
                return Result.Failure<string>(
                    Domain.Payments.Errors.PaymentErrors.InvalidProvider);
            }

            var request = new PaymentRequest
            {
                Amount = amount,
                MethodType = methodType,
                PaymentMethodToken = paymentMethodToken,
                ExternalReference = externalReference,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            return await paymentProvider.ProcessPaymentAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment with provider {Provider}", provider);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public async Task<Result<string>> RefundPaymentAsync(
        string transactionId,
        Money amount = null,
        string reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Implementation would depend on provider identification from transaction ID
            // For now, try all providers until one handles it
            foreach (var provider in _providers.Values)
            {
                var result = await provider.RefundPaymentAsync(transactionId, amount, reason, cancellationToken);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed("No provider could process the refund"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", transactionId);
            return Result.Failure<string>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }

    public async Task<Result<PaymentStatus>> GetPaymentStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Implementation would depend on provider identification from transaction ID
            // For now, try all providers until one handles it
            foreach (var provider in _providers.Values)
            {
                var result = await provider.GetPaymentStatusAsync(transactionId, cancellationToken);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            return Result.Failure<PaymentStatus>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed("No provider could get the payment status"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for transaction {TransactionId}", transactionId);
            return Result.Failure<PaymentStatus>(
                Domain.Payments.Errors.PaymentErrors.ProcessingFailed(ex.Message));
        }
    }
}