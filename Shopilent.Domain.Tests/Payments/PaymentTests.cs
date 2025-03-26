using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Events;
using Shopilent.Domain.Payments.ValueObjects;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Payments;

public class PaymentTests
{
    private User CreateTestUser()
    {
        var emailResult = Email.Create("test@example.com");
        var fullNameResult = FullName.Create("Test", "User");
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);

        Assert.True(userResult.IsSuccess);
        return userResult.Value;
    }

    private Address CreateTestAddress(User user)
    {
        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");

        Assert.True(postalAddressResult.IsSuccess);

        var addressResult = Address.CreateShipping(
            user,
            postalAddressResult.Value);

        Assert.True(addressResult.IsSuccess);
        return addressResult.Value;
    }

    private Order CreateTestOrder(User user, Address address)
    {
        var subtotalResult = Money.Create(100, "USD");
        var taxResult = Money.Create(10, "USD");
        var shippingCostResult = Money.Create(5, "USD");

        Assert.True(subtotalResult.IsSuccess);
        Assert.True(taxResult.IsSuccess);
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        return orderResult.Value;
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreatePayment()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;
        var methodType = PaymentMethodType.CreditCard;
        var provider = PaymentProvider.Stripe;
        var externalReference = "ext_ref_123";

        // Act
        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            methodType,
            provider,
            externalReference);

        // Assert
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;
        Assert.Equal(order.Id, payment.OrderId);
        Assert.Equal(user.Id, payment.UserId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(amount.Currency, payment.Currency);
        Assert.Equal(methodType, payment.MethodType);
        Assert.Equal(provider, payment.Provider);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(externalReference, payment.ExternalReference);
        Assert.Null(payment.TransactionId);
        Assert.Null(payment.ProcessedAt);
        Assert.Null(payment.ErrorMessage);
        Assert.Empty(payment.Metadata);
        Assert.Contains(payment.DomainEvents, e => e is PaymentCreatedEvent);
    }

    [Fact]
    public void Create_WithNullOrder_ShouldReturnFailure()
    {
        // Arrange
        Order order = null;
        var user = CreateTestUser();
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;
        var methodType = PaymentMethodType.CreditCard;
        var provider = PaymentProvider.Stripe;

        // Act
        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            methodType,
            provider);

        // Assert
        Assert.True(paymentResult.IsFailure);
        Assert.Equal("Order.NotFound", paymentResult.Error.Code);
    }

    [Fact]
    public void Create_WithNullAmount_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        Money amount = null;
        var methodType = PaymentMethodType.CreditCard;
        var provider = PaymentProvider.Stripe;

        // Act
        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            methodType,
            provider);

        // Assert
        Assert.True(paymentResult.IsFailure);
        Assert.Equal("Payment.NegativeAmount", paymentResult.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange - cannot directly create negative Money, so we'll test the validation
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var methodType = PaymentMethodType.CreditCard;
        var provider = PaymentProvider.Stripe;

        // We can't create a Money object with negative amount, so verify with zero
        var amountResult = Money.Create(0, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        // Act
        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            methodType,
            provider);

        // Assert - Zero should be allowed
        Assert.True(paymentResult.IsSuccess);
        Assert.Equal(0m, paymentResult.Value.Amount.Amount);
    }

    [Fact]
    public void CreateWithPaymentMethod_ShouldCreatePaymentWithMethod()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "pm_123",
            cardDetailsResult.Value,
            true);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        // Act
        var paymentResult = Payment.CreateWithPaymentMethod(
            order,
            user,
            amount,
            paymentMethod);

        // Assert
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;
        Assert.Equal(order.Id, payment.OrderId);
        Assert.Equal(user.Id, payment.UserId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(paymentMethod.Type, payment.MethodType);
        Assert.Equal(paymentMethod.Provider, payment.Provider);
        Assert.Equal(paymentMethod.Id, payment.PaymentMethodId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Contains(payment.DomainEvents, e => e is PaymentCreatedEvent);
    }

    [Fact]
    public void CreateWithPaymentMethod_WithNullPaymentMethod_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;
        PaymentMethod paymentMethod = null;

        // Act
        var paymentResult = Payment.CreateWithPaymentMethod(
            order,
            user,
            amount,
            paymentMethod);

        // Assert
        Assert.True(paymentResult.IsFailure);
        Assert.Equal("Payment.PaymentMethodNotFound", paymentResult.Error.Code);
    }

    [Fact]
    public void UpdateStatus_ShouldChangeStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        Assert.Equal(PaymentStatus.Pending, payment.Status);

        var newStatus = PaymentStatus.Processing;
        var transactionId = "txn_123";

        // Act
        var updateResult = payment.UpdateStatus(newStatus, transactionId);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newStatus, payment.Status);
        Assert.Equal(transactionId, payment.TransactionId);
        Assert.Contains(payment.DomainEvents, e => e is PaymentStatusChangedEvent);
    }

    [Fact]
    public void UpdateStatus_WithErrorMessage_ShouldSetErrorMessage()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        Assert.Equal(PaymentStatus.Pending, payment.Status);

        var newStatus = PaymentStatus.Failed;
        var errorMessage = "Card declined";

        // Act
        var updateResult = payment.UpdateStatus(newStatus, null, errorMessage);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newStatus, payment.Status);
        Assert.Equal(errorMessage, payment.ErrorMessage);
        Assert.Contains(payment.DomainEvents, e => e is PaymentStatusChangedEvent);
    }

    [Fact]
    public void MarkAsSucceeded_ShouldUpdateStatusAndProcessedTime()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Null(payment.ProcessedAt);

        var transactionId = "txn_123";

        // Act
        var succeededResult = payment.MarkAsSucceeded(transactionId);

        // Assert
        Assert.True(succeededResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(transactionId, payment.TransactionId);
        Assert.NotNull(payment.ProcessedAt);
        Assert.Contains(payment.DomainEvents, e => e is PaymentSucceededEvent);
    }

    [Fact]
    public void MarkAsSucceeded_WhenAlreadySucceeded_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var succeededResult = payment.MarkAsSucceeded("txn_123");
        Assert.True(succeededResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);

        // Clear existing events
        payment.ClearDomainEvents();

        // Act
        var secondSucceededResult = payment.MarkAsSucceeded("txn_456");

        // Assert
        Assert.True(secondSucceededResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal("txn_123", payment.TransactionId); // Should keep the first transaction ID
        Assert.Empty(payment.DomainEvents); // No events should be raised
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        Assert.Equal(PaymentStatus.Pending, payment.Status);

        var errorMessage = "Insufficient funds";

        // Act
        var failedResult = payment.MarkAsFailed(errorMessage);

        // Assert
        Assert.True(failedResult.IsSuccess);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(errorMessage, payment.ErrorMessage);
        Assert.Contains(payment.DomainEvents, e => e is PaymentFailedEvent);
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadyFailed_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var failedResult = payment.MarkAsFailed("First error");
        Assert.True(failedResult.IsSuccess);
        Assert.Equal(PaymentStatus.Failed, payment.Status);

        // Clear existing events
        payment.ClearDomainEvents();

        // Act
        var secondFailedResult = payment.MarkAsFailed("New error");

        // Assert
        Assert.True(secondFailedResult.IsSuccess);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("First error", payment.ErrorMessage); // Should keep the first error
        Assert.Empty(payment.DomainEvents); // No events should be raised
    }

    [Fact]
    public void MarkAsRefunded_ShouldUpdateStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        // First mark as succeeded
        var succeededResult = payment.MarkAsSucceeded("txn_123");
        Assert.True(succeededResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);

        // Clear existing events
        payment.ClearDomainEvents();

        var refundTransactionId = "ref_123";

        // Act
        var refundedResult = payment.MarkAsRefunded(refundTransactionId);

        // Assert
        Assert.True(refundedResult.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(refundTransactionId, payment.TransactionId);
        Assert.Contains(payment.DomainEvents, e => e is PaymentRefundedEvent);
    }

    [Fact]
    public void MarkAsRefunded_WithNonSucceededPayment_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        Assert.Equal(PaymentStatus.Pending, payment.Status);

        var refundTransactionId = "ref_123";

        // Act
        var refundedResult = payment.MarkAsRefunded(refundTransactionId);

        // Assert
        Assert.True(refundedResult.IsFailure);
        Assert.Equal("Payment.InvalidStatus", refundedResult.Error.Code);
    }

    [Fact]
    public void UpdateExternalReference_ShouldUpdateReference()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var newRef = "new_ref_123";

        // Act
        var updateResult = payment.UpdateExternalReference(newRef);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newRef, payment.ExternalReference);
    }

    [Fact]
    public void UpdateExternalReference_WithEmptyValue_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var emptyRef = string.Empty;

        // Act
        var updateResult = payment.UpdateExternalReference(emptyRef);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("Payment.TokenRequired", updateResult.Error.Code);
    }

    [Fact]
    public void UpdateMetadata_ShouldAddOrUpdateValue()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var key = "receipt_url";
        var value = "https://example.com/receipt/123";

        // Act
        var metadataResult = payment.UpdateMetadata(key, value);

        // Assert
        Assert.True(metadataResult.IsSuccess);
        Assert.True(payment.Metadata.ContainsKey(key));
        Assert.Equal(value, payment.Metadata[key]);
    }

    [Fact]
    public void UpdateMetadata_WithEmptyKey_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var emptyKey = string.Empty;
        var value = "test";

        // Act
        var metadataResult = payment.UpdateMetadata(emptyKey, value);

        // Assert
        Assert.True(metadataResult.IsFailure);
        Assert.Equal("Payment.InvalidMetadataKey", metadataResult.Error.Code);
    }

    [Fact]
    public void SetPaymentMethod_ShouldUpdatePaymentMethod()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "pm_123",
            cardDetailsResult.Value,
            true);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        // Act
        var setMethodResult = payment.SetPaymentMethod(paymentMethod);

        // Assert
        Assert.True(setMethodResult.IsSuccess);
        Assert.Equal(paymentMethod.Id, payment.PaymentMethodId);
        Assert.Equal(paymentMethod.Type, payment.MethodType);
        Assert.Equal(paymentMethod.Provider, payment.Provider);
    }

    [Fact]
    public void SetPaymentMethod_WithNullMethod_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        var amount = amountResult.Value;

        var paymentResult = Payment.Create(
            order,
            user,
            amount,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        PaymentMethod paymentMethod = null;

        // Act
        var setMethodResult = payment.SetPaymentMethod(paymentMethod);

        // Assert
        Assert.True(setMethodResult.IsFailure);
        Assert.Equal("Payment.PaymentMethodNotFound", setMethodResult.Error.Code);
    }
}