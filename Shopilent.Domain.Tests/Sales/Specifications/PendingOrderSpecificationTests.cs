using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Specifications;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Sales.Specifications;

public class PendingOrderSpecificationTests
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
    public void IsSatisfiedBy_WithPendingOrder_ShouldReturnTrue()
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
        
        var specification = new PendingOrderSpecification();

        // Act
        var result = specification.IsSatisfiedBy(order);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithPaidOrder_ShouldReturnFalse()
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
        
        var specification = new PendingOrderSpecification();

        // Act
        var result = specification.IsSatisfiedBy(order);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithShippedOrder_ShouldReturnFalse()
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
        
        var specification = new PendingOrderSpecification();

        // Act
        var result = specification.IsSatisfiedBy(order);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithCancelledOrder_ShouldReturnFalse()
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
        
        var cancelResult = order.Cancel();
        Assert.True(cancelResult.IsSuccess);
        
        var specification = new PendingOrderSpecification();

        // Act
        var result = specification.IsSatisfiedBy(order);

        // Assert
        Assert.False(result);
    }
}