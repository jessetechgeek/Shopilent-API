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
}