using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Events;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Payments.Events;

public class PaymentEventsTests
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
    public void Payment_WhenCreated_ShouldRaisePaymentCreatedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);

        // Act
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        
        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);

        // Assert
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;
        var domainEvent = Assert.Single(payment.DomainEvents, e => e is PaymentCreatedEvent);
        var createdEvent = (PaymentCreatedEvent)domainEvent;
        Assert.Equal(payment.Id, createdEvent.PaymentId);
    }

    [Fact]
    public void Payment_WhenStatusChanged_ShouldRaisePaymentStatusChangedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
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
        
        payment.ClearDomainEvents(); // Clear the creation event
        
        var oldStatus = payment.Status;
        var newStatus = PaymentStatus.Processing;

        // Act
        var updateResult = payment.UpdateStatus(newStatus);
        Assert.True(updateResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(payment.DomainEvents, e => e is PaymentStatusChangedEvent);
        var statusEvent = (PaymentStatusChangedEvent)domainEvent;
        Assert.Equal(payment.Id, statusEvent.PaymentId);
        Assert.Equal(oldStatus, statusEvent.OldStatus);
        Assert.Equal(newStatus, statusEvent.NewStatus);
    }

    [Fact]
    public void Payment_WhenMarkedAsSucceeded_ShouldRaisePaymentSucceededEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
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
        
        payment.ClearDomainEvents(); // Clear the creation event
        var transactionId = "txn_123";

        // Act
        var succeededResult = payment.MarkAsSucceeded(transactionId);
        Assert.True(succeededResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(payment.DomainEvents, e => e is PaymentSucceededEvent);
        var succeededEvent = (PaymentSucceededEvent)domainEvent;
        Assert.Equal(payment.Id, succeededEvent.PaymentId);
        Assert.Equal(order.Id, succeededEvent.OrderId);
    }

    [Fact]
    public void Payment_WhenMarkedAsFailed_ShouldRaisePaymentFailedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
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
        
        payment.ClearDomainEvents(); // Clear the creation event
        var errorMessage = "Card declined";

        // Act
        var failedResult = payment.MarkAsFailed(errorMessage);
        Assert.True(failedResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(payment.DomainEvents, e => e is PaymentFailedEvent);
        var failedEvent = (PaymentFailedEvent)domainEvent;
        Assert.Equal(payment.Id, failedEvent.PaymentId);
        Assert.Equal(order.Id, failedEvent.OrderId);
        Assert.Equal(errorMessage, failedEvent.ErrorMessage);
    }

    [Fact]
    public void Payment_WhenMarkedAsRefunded_ShouldRaisePaymentRefundedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
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
        
        // First mark as succeeded
        var succeededResult = payment.MarkAsSucceeded("txn_123");
        Assert.True(succeededResult.IsSuccess);
        
        payment.ClearDomainEvents(); // Clear previous events
        var refundId = "ref_123";

        // Act
        var refundedResult = payment.MarkAsRefunded(refundId);
        Assert.True(refundedResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(payment.DomainEvents, e => e is PaymentRefundedEvent);
        var refundedEvent = (PaymentRefundedEvent)domainEvent;
        Assert.Equal(payment.Id, refundedEvent.PaymentId);
        Assert.Equal(order.Id, refundedEvent.OrderId);
    }
}