using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Events;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.ValueObjects;
using Shopilent.Domain.Tests.Common;

namespace Shopilent.Domain.Tests.Sales.Events;

public class OrderEventTests
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

    [Fact]
    public void Order_WhenCreated_ShouldRaiseOrderCreatedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);

        var subtotalResult = Money.FromDollars(100);
        Assert.True(subtotalResult.IsSuccess);
        
        var taxResult = Money.FromDollars(10);
        Assert.True(taxResult.IsSuccess);
        
        var shippingCostResult = Money.FromDollars(5);
        Assert.True(shippingCostResult.IsSuccess);

        // Act
        var orderResult = Order.Create(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        // Assert
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        var domainEvent = Assert.Single(order.DomainEvents, e => e is OrderCreatedEvent);
        var createdEvent = (OrderCreatedEvent)domainEvent;
        Assert.Equal(order.Id, createdEvent.OrderId);
    }

    [Fact]
    public void Order_WhenStatusChanged_ShouldRaiseOrderStatusChangedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        
        var subtotalResult = Money.FromDollars(100);
        Assert.True(subtotalResult.IsSuccess);
        
        var taxResult = Money.FromDollars(10);
        Assert.True(taxResult.IsSuccess);
        
        var shippingCostResult = Money.FromDollars(5);
        Assert.True(shippingCostResult.IsSuccess);
        
        var orderResult = Order.Create(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);
            
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        order.ClearDomainEvents(); // Clear the creation event

        // Act
        var updateResult = order.UpdateOrderStatus(OrderStatus.Processing);
        Assert.True(updateResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(order.DomainEvents, e => e is OrderStatusChangedEvent);
        var statusEvent = (OrderStatusChangedEvent)domainEvent;
        Assert.Equal(order.Id, statusEvent.OrderId);
        Assert.Equal(OrderStatus.Pending, statusEvent.OldStatus);
        Assert.Equal(OrderStatus.Processing, statusEvent.NewStatus);
    }

    [Fact]
    public void Order_WhenPaymentStatusChanged_ShouldRaiseOrderPaymentStatusChangedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        
        var subtotalResult = Money.FromDollars(100);
        Assert.True(subtotalResult.IsSuccess);
        
        var taxResult = Money.FromDollars(10);
        Assert.True(taxResult.IsSuccess);
        
        var shippingCostResult = Money.FromDollars(5);
        Assert.True(shippingCostResult.IsSuccess);
        
        var orderResult = Order.Create(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);
            
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        order.ClearDomainEvents(); // Clear the creation event

        // Act
        var updateResult = order.UpdatePaymentStatus(PaymentStatus.Succeeded);
        Assert.True(updateResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(order.DomainEvents, e => e is OrderPaymentStatusChangedEvent);
        var paymentEvent = (OrderPaymentStatusChangedEvent)domainEvent;
        Assert.Equal(order.Id, paymentEvent.OrderId);
        Assert.Equal(PaymentStatus.Pending, paymentEvent.OldStatus);
        Assert.Equal(PaymentStatus.Succeeded, paymentEvent.NewStatus);
    }

    [Fact]
    public void Order_WhenMarkedAsPaid_ShouldRaiseOrderPaidEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        
        var subtotalResult = Money.FromDollars(100);
        Assert.True(subtotalResult.IsSuccess);
        
        var taxResult = Money.FromDollars(10);
        Assert.True(taxResult.IsSuccess);
        
        var shippingCostResult = Money.FromDollars(5);
        Assert.True(shippingCostResult.IsSuccess);
        
        var orderResult = Order.Create(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);
            
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        order.ClearDomainEvents(); // Clear the creation event

        // Act
        var paidResult = order.MarkAsPaid();
        Assert.True(paidResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(order.DomainEvents, e => e is OrderPaidEvent);
        var paidEvent = (OrderPaidEvent)domainEvent;
        Assert.Equal(order.Id, paidEvent.OrderId);

        // Also check status events are raised
        Assert.Contains(order.DomainEvents, e => e is OrderPaymentStatusChangedEvent);
        Assert.Contains(order.DomainEvents, e => e is OrderStatusChangedEvent);
    }

    [Fact]
    public void Order_WhenMarkedAsShipped_ShouldRaiseOrderShippedEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        
        var subtotalResult = Money.FromDollars(100);
        Assert.True(subtotalResult.IsSuccess);
        
        var taxResult = Money.FromDollars(10);
        Assert.True(taxResult.IsSuccess);
        
        var shippingCostResult = Money.FromDollars(5);
        Assert.True(shippingCostResult.IsSuccess);
        
        var orderResult = Order.CreatePaidOrder(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);
            
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        order.ClearDomainEvents(); // Clear previous events

        // Act
        var shippedResult = order.MarkAsShipped("TRACK123");
        Assert.True(shippedResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(order.DomainEvents, e => e is OrderShippedEvent);
        var shippedEvent = (OrderShippedEvent)domainEvent;
        Assert.Equal(order.Id, shippedEvent.OrderId);
    }

    [Fact]
    public void Order_WhenMarkedAsDelivered_ShouldRaiseOrderDeliveredEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        
        var subtotalResult = Money.FromDollars(100);
        Assert.True(subtotalResult.IsSuccess);
        
        var taxResult = Money.FromDollars(10);
        Assert.True(taxResult.IsSuccess);
        
        var shippingCostResult = Money.FromDollars(5);
        Assert.True(shippingCostResult.IsSuccess);
        
        var orderResult = Order.CreatePaidOrder(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);
            
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        
        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);
        
        order.ClearDomainEvents(); // Clear previous events

        // Act
        var deliveredResult = order.MarkAsDelivered();
        Assert.True(deliveredResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(order.DomainEvents, e => e is OrderDeliveredEvent);
        var deliveredEvent = (OrderDeliveredEvent)domainEvent;
        Assert.Equal(order.Id, deliveredEvent.OrderId);
    }

    [Fact]
    public void Order_WhenCancelled_ShouldRaiseOrderCancelledEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        
        var subtotalResult = Money.FromDollars(100);
        Assert.True(subtotalResult.IsSuccess);
        
        var taxResult = Money.FromDollars(10);
        Assert.True(taxResult.IsSuccess);
        
        var shippingCostResult = Money.FromDollars(5);
        Assert.True(shippingCostResult.IsSuccess);
        
        var orderResult = Order.Create(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);
            
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        order.ClearDomainEvents(); // Clear the creation event

        // Act
        var cancelResult = order.Cancel("Customer request");
        Assert.True(cancelResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(order.DomainEvents, e => e is OrderCancelledEvent);
        var cancelledEvent = (OrderCancelledEvent)domainEvent;
        Assert.Equal(order.Id, cancelledEvent.OrderId);
    }
}