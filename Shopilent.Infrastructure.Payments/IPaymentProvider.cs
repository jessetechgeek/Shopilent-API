using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Infrastructure.Payments;

public interface IPaymentProvider
{
    PaymentProvider Provider { get; }
    
    Task<Result<string>> ProcessPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<string>> RefundPaymentAsync(
        string transactionId,
        Money amount = null,
        string reason = null,
        CancellationToken cancellationToken = default);
    
    Task<Result<PaymentStatus>> GetPaymentStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
    
    // Customer management methods - optional for providers that support it
    Task<Result<string>> CreateCustomerAsync(
        string userId,
        string email,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<string>(
            Domain.Common.Errors.Error.Failure(
                code: "CustomerManagement.NotSupported",
                message: $"Customer management is not supported by {Provider} provider")));
    }
    
    Task<Result<string>> AttachPaymentMethodToCustomerAsync(
        string paymentMethodToken,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<string>(
            Domain.Common.Errors.Error.Failure(
                code: "CustomerManagement.NotSupported", 
                message: $"Payment method attachment is not supported by {Provider} provider")));
    }
}