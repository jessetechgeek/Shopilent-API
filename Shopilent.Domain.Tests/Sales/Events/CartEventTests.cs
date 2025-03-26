using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Events;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Sales.Events;

public class CartEventTests
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

    private Product CreateTestProduct()
    {
        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);
        
        var priceResult = Money.FromDollars(100);
        Assert.True(priceResult.IsSuccess);
        
        var productResult = Product.Create(
            "Test Product",
            slugResult.Value,
            priceResult.Value);
            
        Assert.True(productResult.IsSuccess);
        return productResult.Value;
    }

    [Fact]
    public void Cart_WhenCreated_ShouldRaiseCartCreatedEvent()
    {
        // Act
        var cartResult = Cart.Create();

        // Assert
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        var domainEvent = Assert.Single(cart.DomainEvents, e => e is CartCreatedEvent);
        var createdEvent = (CartCreatedEvent)domainEvent;
        Assert.Equal(cart.Id, createdEvent.CartId);
    }

    [Fact]
    public void Cart_WhenAssignedToUser_ShouldRaiseCartAssignedToUserEvent()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        
        var user = CreateTestUser();
        cart.ClearDomainEvents(); // Clear the creation event

        // Act
        var assignResult = cart.AssignToUser(user);

        // Assert
        Assert.True(assignResult.IsSuccess);
        var domainEvent = Assert.Single(cart.DomainEvents, e => e is CartAssignedToUserEvent);
        var assignedEvent = (CartAssignedToUserEvent)domainEvent;
        Assert.Equal(cart.Id, assignedEvent.CartId);
        Assert.Equal(user.Id, assignedEvent.UserId);
    }

    [Fact]
    public void Cart_WhenItemAdded_ShouldRaiseCartItemAddedEvent()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        
        var product = CreateTestProduct();
        cart.ClearDomainEvents(); // Clear the creation event

        // Act
        var cartItemResult = cart.AddItem(product, 2);

        // Assert
        Assert.True(cartItemResult.IsSuccess);
        var cartItem = cartItemResult.Value;
        var domainEvent = Assert.Single(cart.DomainEvents, e => e is CartItemAddedEvent);
        var addedEvent = (CartItemAddedEvent)domainEvent;
        Assert.Equal(cart.Id, addedEvent.CartId);
        Assert.Equal(cartItem.Id, addedEvent.ItemId);
    }

    [Fact]
    public void Cart_WhenItemUpdated_ShouldRaiseCartItemUpdatedEvent()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        
        var product = CreateTestProduct();
        var cartItemResult = cart.AddItem(product, 1);
        Assert.True(cartItemResult.IsSuccess);
        var cartItem = cartItemResult.Value;
        
        cart.ClearDomainEvents(); // Clear previous events

        // Act
        var updateResult = cart.UpdateItemQuantity(cartItem.Id, 3);

        // Assert
        Assert.True(updateResult.IsSuccess);
        var domainEvent = Assert.Single(cart.DomainEvents, e => e is CartItemUpdatedEvent);
        var updatedEvent = (CartItemUpdatedEvent)domainEvent;
        Assert.Equal(cart.Id, updatedEvent.CartId);
        Assert.Equal(cartItem.Id, updatedEvent.ItemId);
    }

    [Fact]
    public void Cart_WhenItemRemoved_ShouldRaiseCartItemRemovedEvent()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        
        var product = CreateTestProduct();
        var cartItemResult = cart.AddItem(product, 1);
        Assert.True(cartItemResult.IsSuccess);
        var cartItem = cartItemResult.Value;
        
        cart.ClearDomainEvents(); // Clear previous events

        // Act
        var removeResult = cart.RemoveItem(cartItem.Id);

        // Assert
        Assert.True(removeResult.IsSuccess);
        var domainEvent = Assert.Single(cart.DomainEvents, e => e is CartItemRemovedEvent);
        var removedEvent = (CartItemRemovedEvent)domainEvent;
        Assert.Equal(cart.Id, removedEvent.CartId);
        Assert.Equal(cartItem.Id, removedEvent.ItemId);
    }

    [Fact]
    public void Cart_WhenCleared_ShouldRaiseCartClearedEvent()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        
        var product1 = CreateTestProduct();
        
        var slugResult = Slug.Create("product-2");
        Assert.True(slugResult.IsSuccess);
        
        var priceResult = Money.FromDollars(200);
        Assert.True(priceResult.IsSuccess);
        
        var product2Result = Product.Create("Product 2", slugResult.Value, priceResult.Value);
        Assert.True(product2Result.IsSuccess);
        var product2 = product2Result.Value;

        cart.AddItem(product1, 1);
        cart.AddItem(product2, 1);

        cart.ClearDomainEvents(); // Clear previous events

        // Act
        var clearResult = cart.Clear();

        // Assert
        Assert.True(clearResult.IsSuccess);
        var domainEvent = Assert.Single(cart.DomainEvents, e => e is CartClearedEvent);
        var clearedEvent = (CartClearedEvent)domainEvent;
        Assert.Equal(cart.Id, clearedEvent.CartId);
    }
}