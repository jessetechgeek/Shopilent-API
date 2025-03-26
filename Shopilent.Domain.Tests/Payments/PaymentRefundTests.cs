using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Events;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Payments;

public class PaymentRefundTests
{
    private User CreateTestUser()
    {
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);

        var fullNameResult = FullName.Create("Test", "User");
        Assert.True(fullNameResult.IsSuccess);

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

    private Payment CreateTestPayment(Order order, User user)
    {
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);

        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);

        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        // Mark as succeeded before refund tests
        var succeededResult = payment.MarkAsSucceeded("txn_123");
        Assert.True(succeededResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);

        payment.ClearDomainEvents(); // Clear previous events
        return payment;
    }

    [Fact]
    public void MarkAsRefunded_WithSucceededPayment_ShouldUpdateStatusAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var payment = CreateTestPayment(order, user);
        var refundTransactionId = "ref_123";

        // Act
        var refundResult = payment.MarkAsRefunded(refundTransactionId);

        // Assert
        Assert.True(refundResult.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(refundTransactionId, payment.TransactionId);
        
        var domainEvent = Assert.Single(payment.DomainEvents, e => e is PaymentRefundedEvent);
        var refundedEvent = (PaymentRefundedEvent)domainEvent;
        Assert.Equal(payment.Id, refundedEvent.PaymentId);
        Assert.Equal(order.Id, refundedEvent.OrderId);
    }

    [Fact]
    public void MarkAsRefunded_WithPendingPayment_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        
        // Create payment but don't mark as succeeded
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);

        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;
        
        var refundTransactionId = "ref_123";

        // Act
        var refundResult = payment.MarkAsRefunded(refundTransactionId);

        // Assert
        Assert.True(refundResult.IsFailure);
        Assert.Equal("Payment.InvalidStatus", refundResult.Error.Code);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void MarkAsRefunded_WithEmptyTransactionId_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var payment = CreateTestPayment(order, user);
        var emptyTransactionId = string.Empty;

        // Act
        var refundResult = payment.MarkAsRefunded(emptyTransactionId);

        // Assert
        Assert.True(refundResult.IsFailure);
        Assert.Equal("Payment.TokenRequired", refundResult.Error.Code);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status); // Unchanged
    }

    [Fact]
    public void MarkAsRefunded_WhenAlreadyRefunded_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var payment = CreateTestPayment(order, user);
        
        // First refund
        var firstResult = payment.MarkAsRefunded("ref_123");
        Assert.True(firstResult.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        
        payment.ClearDomainEvents(); // Clear events from first refund
        
        // Act - attempt second refund
        var secondResult = payment.MarkAsRefunded("ref_456");

        // Assert
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal("ref_123", payment.TransactionId); // Original transaction ID should be preserved
        Assert.Empty(payment.DomainEvents); // No events should be raised
    }
}