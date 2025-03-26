using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Events;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Tests.Sales;

public class CartTests
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

    private Product CreateTestProduct(string name = "Test Product", decimal price = 100M)
    {
        var slugResult = Slug.Create(name.ToLower().Replace(" ", "-"));
        Assert.True(slugResult.IsSuccess);

        var priceResult = Money.FromDollars(price);
        Assert.True(priceResult.IsSuccess);

        var productResult = Product.Create(
            name,
            slugResult.Value,
            priceResult.Value);

        Assert.True(productResult.IsSuccess);
        return productResult.Value;
    }

    [Fact]
    public void Create_WithoutUser_ShouldCreateEmptyCart()
    {
        // Act
        var cartResult = Cart.Create();

        // Assert
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        Assert.Null(cart.UserId);
        Assert.Empty(cart.Items);
        Assert.Empty(cart.Metadata);
        Assert.Contains(cart.DomainEvents, e => e is CartCreatedEvent);
    }

    [Fact]
    public void Create_WithUser_ShouldCreateEmptyCartForUser()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var cartResult = Cart.Create(user);

        // Assert
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        Assert.Equal(user.Id, cart.UserId);
        Assert.Empty(cart.Items);
        Assert.Empty(cart.Metadata);
        Assert.Contains(cart.DomainEvents, e => e is CartCreatedEvent);
    }

    [Fact]
    public void CreateWithMetadata_ShouldCreateCartWithMetadata()
    {
        // Arrange
        var user = CreateTestUser();
        var metadata = new Dictionary<string, object>
        {
            { "source", "mobile_app" },
            { "version", "1.0" }
        };

        // Act
        var cartResult = Cart.CreateWithMetadata(user, metadata);

        // Assert
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        Assert.Equal(user.Id, cart.UserId);
        Assert.Empty(cart.Items);
        Assert.Equal(2, cart.Metadata.Count);
        Assert.Equal("mobile_app", cart.Metadata["source"]);
        Assert.Equal("1.0", cart.Metadata["version"]);
        Assert.Contains(cart.DomainEvents, e => e is CartCreatedEvent);
    }

    [Fact]
    public void AssignToUser_ShouldAssignCartToUser()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        Assert.Null(cart.UserId);

        var user = CreateTestUser();

        // Act
        var assignResult = cart.AssignToUser(user);

        // Assert
        Assert.True(assignResult.IsSuccess);
        Assert.Equal(user.Id, cart.UserId);
        Assert.Contains(cart.DomainEvents, e => e is CartAssignedToUserEvent);
    }

    [Fact]
    public void AssignToUser_WithNullUser_ShouldReturnFailure()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;
        User user = null;

        // Act
        var assignResult = cart.AssignToUser(user);

        // Assert
        Assert.True(assignResult.IsFailure);
        Assert.Equal("User.NotFound", assignResult.Error.Code);
    }

    [Fact]
    public void AddItem_NewProduct_ShouldAddItemToCart()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var product = CreateTestProduct();
        var quantity = 2;

        // Act
        var cartItemResult = cart.AddItem(product, quantity);

        // Assert
        Assert.True(cartItemResult.IsSuccess);
        var cartItem = cartItemResult.Value;
        Assert.Single(cart.Items);
        Assert.Equal(product.Id, cartItem.ProductId);
        Assert.Equal(quantity, cartItem.Quantity);
        Assert.Null(cartItem.VariantId);
        Assert.Contains(cart.DomainEvents, e => e is CartItemAddedEvent);
    }

    [Fact]
    public void AddItem_ExistingProduct_ShouldIncreaseQuantity()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var product = CreateTestProduct();

        // Add first item
        var initialQuantity = 2;
        var cartItemResult = cart.AddItem(product, initialQuantity);
        Assert.True(cartItemResult.IsSuccess);
        var cartItem = cartItemResult.Value;
        Assert.Equal(initialQuantity, cartItem.Quantity);

        // Add same product again
        var additionalQuantity = 3;

        // Act
        var updatedItemResult = cart.AddItem(product, additionalQuantity);

        // Assert
        Assert.True(updatedItemResult.IsSuccess);
        var updatedItem = updatedItemResult.Value;
        Assert.Single(cart.Items);
        Assert.Equal(cartItem.Id, updatedItem.Id);
        Assert.Equal(initialQuantity + additionalQuantity, updatedItem.Quantity);
    }

    [Fact]
    public void AddItem_WithProductVariant_ShouldAddItemWithVariant()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var product = CreateTestProduct();

        var priceResult = Money.FromDollars(150);
        Assert.True(priceResult.IsSuccess);

        var variantResult = ProductVariant.Create(product.Id, "VAR-123", priceResult.Value, 100);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var quantity = 1;

        // Act
        var cartItemResult = cart.AddItem(product, quantity, variant);

        // Assert
        Assert.True(cartItemResult.IsSuccess);
        var cartItem = cartItemResult.Value;
        Assert.Single(cart.Items);
        Assert.Equal(product.Id, cartItem.ProductId);
        Assert.Equal(variant.Id, cartItem.VariantId);
        Assert.Equal(quantity, cartItem.Quantity);
        Assert.Contains(cart.DomainEvents, e => e is CartItemAddedEvent);
    }

    [Fact]
    public void AddItem_WithNullProduct_ShouldReturnFailure()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        Product product = null;
        var quantity = 1;

        // Act
        var cartItemResult = cart.AddItem(product, quantity);

        // Assert
        Assert.True(cartItemResult.IsFailure);
        Assert.Equal("Product.NotFound", cartItemResult.Error.Code);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var product = CreateTestProduct();
        var quantity = 0;

        // Act
        var cartItemResult = cart.AddItem(product, quantity);

        // Assert
        Assert.True(cartItemResult.IsFailure);
        Assert.Equal("Cart.InvalidQuantity", cartItemResult.Error.Code);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldUpdateItemQuantity()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var product = CreateTestProduct();
        var initialQuantity = 1;
        var cartItemResult = cart.AddItem(product, initialQuantity);
        Assert.True(cartItemResult.IsSuccess);
        var cartItem = cartItemResult.Value;

        var newQuantity = 5;

        // Act
        var updateResult = cart.UpdateItemQuantity(cartItem.Id, newQuantity);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Single(cart.Items);
        Assert.Equal(newQuantity, cart.Items.First().Quantity);
        Assert.Contains(cart.DomainEvents, e => e is CartItemUpdatedEvent);
    }

    [Fact]
    public void UpdateItemQuantity_WithZeroOrLessQuantity_ShouldRemoveItem()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var product = CreateTestProduct();
        var cartItemResult = cart.AddItem(product, 2);
        Assert.True(cartItemResult.IsSuccess);
        Assert.Single(cart.Items);

        // Act
        var updateResult = cart.UpdateItemQuantity(cartItemResult.Value.Id, 0);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Empty(cart.Items);
        Assert.Contains(cart.DomainEvents, e => e is CartItemRemovedEvent);
    }

    [Fact]
    public void UpdateItemQuantity_WithInvalidItemId_ShouldReturnFailure()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var invalidItemId = Guid.NewGuid();
        var quantity = 5;

        // Act
        var updateResult = cart.UpdateItemQuantity(invalidItemId, quantity);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("Cart.ItemNotFound", updateResult.Error.Code);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromCart()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var product = CreateTestProduct();
        var cartItemResult = cart.AddItem(product, 1);
        Assert.True(cartItemResult.IsSuccess);
        Assert.Single(cart.Items);

        // Act
        var removeResult = cart.RemoveItem(cartItemResult.Value.Id);

        // Assert
        Assert.True(removeResult.IsSuccess);
        Assert.Empty(cart.Items);
        Assert.Contains(cart.DomainEvents, e => e is CartItemRemovedEvent);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItemsFromCart()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        cart.AddItem(CreateTestProduct("Product 1"), 1);
        cart.AddItem(CreateTestProduct("Product 2"), 2);
        cart.AddItem(CreateTestProduct("Product 3"), 3);
        Assert.Equal(3, cart.Items.Count);

        // Act
        var clearResult = cart.Clear();

        // Assert
        Assert.True(clearResult.IsSuccess);
        Assert.Empty(cart.Items);
        Assert.Contains(cart.DomainEvents, e => e is CartClearedEvent);
    }

    [Fact]
    public void UpdateMetadata_ShouldAddOrUpdateMetadata()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var key = "campaign";
        var value = "summer_sale";

        // Act
        var updateResult = cart.UpdateMetadata(key, value);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Single(cart.Metadata);
        Assert.Equal(value, cart.Metadata[key]);
    }

    [Fact]
    public void UpdateMetadata_WithEmptyKey_ShouldReturnFailure()
    {
        // Arrange
        var cartResult = Cart.Create();
        Assert.True(cartResult.IsSuccess);
        var cart = cartResult.Value;

        var key = string.Empty;
        var value = "test";

        // Act
        var updateResult = cart.UpdateMetadata(key, value);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("Cart.InvalidMetadataKey", updateResult.Error.Code);
    }
}