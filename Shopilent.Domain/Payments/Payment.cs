using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Errors;
using Shopilent.Domain.Payments.Events;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Errors;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Payments;

public class Payment : AggregateRoot
{
    private Payment()
    {
        // Required by EF Core
    }

    private Payment(
        Order order,
        User user,
        Money amount,
        PaymentMethodType methodType,
        PaymentProvider provider,
        string externalReference = null)
    {
        OrderId = order.Id;
        UserId = user?.Id;
        Amount = amount;
        Currency = amount.Currency; // Set currency from Money object to match DB schema
        MethodType = methodType;
        Provider = provider;
        Status = PaymentStatus.Pending;
        ExternalReference = externalReference;
        Metadata = new Dictionary<string, object>();
    }

    public static Result<Payment> Create(
        Order order,
        User user,
        Money amount,
        PaymentMethodType methodType,
        PaymentProvider provider,
        string externalReference = null)
    {
        if (order == null)
            return Result.Failure<Payment>(OrderErrors.NotFound(Guid.Empty));
            
        if (amount == null)
            return Result.Failure<Payment>(PaymentErrors.NegativeAmount);
            
        if (amount.Amount < 0)
            return Result.Failure<Payment>(PaymentErrors.NegativeAmount);

        var payment = new Payment(order, user, amount, methodType, provider, externalReference);
        payment.AddDomainEvent(new PaymentCreatedEvent(payment.Id));
        return Result.Success(payment);
    }

    // With PaymentMethod entity reference
    public static Result<Payment> CreateWithPaymentMethod(
        Order order,
        User user,
        Money amount,
        PaymentMethod paymentMethod,
        string externalReference = null)
    {
        if (order == null)
            return Result.Failure<Payment>(OrderErrors.NotFound(Guid.Empty));
            
        if (amount == null || amount.Amount <= 0)
            return Result.Failure<Payment>(PaymentErrors.NegativeAmount);
            
        if (paymentMethod == null)
            return Result.Failure<Payment>(PaymentErrors.PaymentMethodNotFound(Guid.Empty));
            
        if (!paymentMethod.IsActive)
            return Result.Failure<Payment>(PaymentMethodErrors.InactivePaymentMethod);

        var payment = new Payment(
            order, 
            user, 
            amount, 
            paymentMethod.Type, 
            paymentMethod.Provider, 
            externalReference);
            
        payment.PaymentMethodId = paymentMethod.Id;
        payment.AddDomainEvent(new PaymentCreatedEvent(payment.Id));
        return Result.Success(payment);
    }

    public Guid OrderId { get; private set; }
    public Guid? UserId { get; private set; }
    public Money Amount { get; private set; }
    public string Currency { get; private set; } // Added to match DB schema
    public PaymentMethodType MethodType { get; private set; } // Renamed from Method to be more clear
    public PaymentProvider Provider { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string ExternalReference { get; private set; }
    public string TransactionId { get; private set; }
    public Guid? PaymentMethodId { get; private set; } // Reference to the payment method entity
    public Dictionary<string, object> Metadata { get; private set; } = new();
    public DateTime? ProcessedAt { get; private set; }
    public string ErrorMessage { get; private set; }

    public Result UpdateStatus(PaymentStatus newStatus, string transactionId = null, string errorMessage = null)
    {
        var oldStatus = Status;
        Status = newStatus;

        if (!string.IsNullOrWhiteSpace(transactionId))
            TransactionId = transactionId;

        if (newStatus == PaymentStatus.Succeeded)
            ProcessedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(errorMessage))
            ErrorMessage = errorMessage;

        AddDomainEvent(new PaymentStatusChangedEvent(Id, oldStatus, newStatus));
        return Result.Success();
    }

    public Result MarkAsSucceeded(string transactionId)
    {
        if (Status == PaymentStatus.Succeeded)
            return Result.Success();
            
        if (string.IsNullOrWhiteSpace(transactionId))
            return Result.Failure(PaymentErrors.TokenRequired);

        UpdateStatus(PaymentStatus.Succeeded, transactionId);
        AddDomainEvent(new PaymentSucceededEvent(Id, OrderId));
        return Result.Success();
    }

    public Result MarkAsFailed(string errorMessage = null)
    {
        if (Status == PaymentStatus.Failed)
            return Result.Success();

        UpdateStatus(PaymentStatus.Failed, null, errorMessage);
        AddDomainEvent(new PaymentFailedEvent(Id, OrderId, errorMessage));
        return Result.Success();
    }

    public Result MarkAsRefunded(string transactionId)
    {
        if (Status == PaymentStatus.Refunded)
            return Result.Success();

        if (Status != PaymentStatus.Succeeded)
            return Result.Failure(PaymentErrors.InvalidPaymentStatus("refund"));
            
        if (string.IsNullOrWhiteSpace(transactionId))
            return Result.Failure(PaymentErrors.TokenRequired);

        UpdateStatus(PaymentStatus.Refunded, transactionId);
        AddDomainEvent(new PaymentRefundedEvent(Id, OrderId));
        return Result.Success();
    }

    public Result UpdateExternalReference(string externalReference)
    {
        if (string.IsNullOrWhiteSpace(externalReference))
            return Result.Failure(PaymentErrors.TokenRequired);

        ExternalReference = externalReference;
        return Result.Success();
    }

    public Result UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure(PaymentErrors.InvalidMetadataKey);

        Metadata[key] = value;
        return Result.Success();
    }

    public Result SetPaymentMethod(PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
            return Result.Failure(PaymentErrors.PaymentMethodNotFound(Guid.Empty));
            
        if (!paymentMethod.IsActive)
            return Result.Failure(PaymentMethodErrors.InactivePaymentMethod);

        PaymentMethodId = paymentMethod.Id;
        MethodType = paymentMethod.Type;
        Provider = paymentMethod.Provider;
        return Result.Success();
    }
}