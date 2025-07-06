using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Application.Abstractions.Services;

public interface IPaymentService
{
    Task<Result<string>> ProcessPaymentAsync(
        Money amount,
        PaymentMethodType methodType,
        PaymentProvider provider,
        string paymentMethodToken,
        string customerId = null,
        string externalReference = null,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default);

    Task<Result<string>> RefundPaymentAsync(
        string transactionId,
        Money amount = null,
        string reason = null,
        CancellationToken cancellationToken = default);

    Task<Result<PaymentStatus>> GetPaymentStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default);

    // Customer management methods
    Task<Result<string>> CreateCustomerAsync(
        PaymentProvider provider,
        string userId,
        string email,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default);

    Task<Result<string>> AttachPaymentMethodToCustomerAsync(
        PaymentProvider provider,
        string paymentMethodToken,
        string customerId,
        CancellationToken cancellationToken = default);
}