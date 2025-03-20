using Shopilent.Domain.Common;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Events;
using Shopilent.Domain.Sales;
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
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount < 0)
            throw new ArgumentException("Payment amount cannot be negative", nameof(amount));

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

    public static Payment Create(
        Order order,
        User user,
        Money amount,
        PaymentMethodType methodType,
        PaymentProvider provider,
        string externalReference = null)
    {
        var payment = new Payment(order, user, amount, methodType, provider, externalReference);
        payment.AddDomainEvent(new PaymentCreatedEvent(payment.Id));
        return payment;
    }

    // With PaymentMethod entity reference
    public static Payment CreateWithPaymentMethod(
        Order order,
        User user,
        Money amount,
        PaymentMethod paymentMethod,
        string externalReference = null)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        var payment = new Payment(order, user, amount, paymentMethod.Type, paymentMethod.Provider, externalReference);
        payment.PaymentMethodId = paymentMethod.Id;
        payment.AddDomainEvent(new PaymentCreatedEvent(payment.Id));
        return payment;
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

    public void UpdateStatus(PaymentStatus newStatus, string transactionId = null, string errorMessage = null)
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
    }

    public void MarkAsSucceeded(string transactionId)
    {
        if (Status == PaymentStatus.Succeeded)
            return;

        UpdateStatus(PaymentStatus.Succeeded, transactionId);
        AddDomainEvent(new PaymentSucceededEvent(Id, OrderId));
    }

    public void MarkAsFailed(string errorMessage = null)
    {
        if (Status == PaymentStatus.Failed)
            return;

        UpdateStatus(PaymentStatus.Failed, null, errorMessage);
        AddDomainEvent(new PaymentFailedEvent(Id, OrderId, errorMessage));
    }

    public void MarkAsRefunded(string transactionId)
    {
        if (Status == PaymentStatus.Refunded)
            return;

        if (Status != PaymentStatus.Succeeded)
            throw new InvalidOperationException("Only successful payments can be refunded");

        UpdateStatus(PaymentStatus.Refunded, transactionId);
        AddDomainEvent(new PaymentRefundedEvent(Id, OrderId));
    }

    public void UpdateExternalReference(string externalReference)
    {
        if (string.IsNullOrWhiteSpace(externalReference))
            throw new ArgumentException("External reference cannot be empty", nameof(externalReference));

        ExternalReference = externalReference;
    }

    public void UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata[key] = value;
    }

    public void SetPaymentMethod(PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        PaymentMethodId = paymentMethod.Id;
        MethodType = paymentMethod.Type;
        Provider = paymentMethod.Provider;
    }
}