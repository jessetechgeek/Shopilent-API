using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Sales;

public class OrderItemTests
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
        var subtotalResult = Money.Create(0, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
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

    private Product CreateTestProduct(string name = "Test Product", decimal price = 50)
    {
        var slugResult = Slug.Create(name.ToLower().Replace(" ", "-"));
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var priceResult = Money.Create(price, "USD");
        Assert.True(priceResult.IsSuccess);
        var basePrice = priceResult.Value;

        var productResult = Product.Create(name, slug, basePrice, "TEST-SKU");
        Assert.True(productResult.IsSuccess);
        return productResult.Value;
    }

    private ProductVariant CreateTestVariant(Product product, string sku = "VAR-SKU", decimal price = 60)
    {
        var priceResult = Money.Create(price, "USD");
        Assert.True(priceResult.IsSuccess);

        var variantResult = ProductVariant.Create(product.Id, sku, priceResult.Value, 10);
        Assert.True(variantResult.IsSuccess);
        return variantResult.Value;
    }

    [Fact]
    public void Create_ShouldCaptureProductDataSnapshot()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var product = CreateTestProduct();
        var quantity = 2;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        // Act
        var orderItemResult = order.AddItem(product, quantity, unitPrice);

        // Assert
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;

        // Verify product data snapshot
        Assert.NotNull(orderItem.ProductData);
        Assert.True(orderItem.ProductData.ContainsKey("name"));
        Assert.Equal(product.Name, orderItem.ProductData["name"]);
        Assert.True(orderItem.ProductData.ContainsKey("sku"));
        Assert.Equal(product.Sku, orderItem.ProductData["sku"]);
        Assert.True(orderItem.ProductData.ContainsKey("slug"));
        Assert.Equal(product.Slug.Value, orderItem.ProductData["slug"]);
    }

    [Fact]
    public void Create_WithVariant_ShouldCaptureVariantDataInSnapshot()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var product = CreateTestProduct();
        var variant = CreateTestVariant(product);
        var quantity = 1;

        var unitPriceResult = Money.Create(60, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        // Act
        var orderItemResult = order.AddItem(product, quantity, unitPrice, variant);

        // Assert
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;

        // Verify variant data in snapshot
        Assert.True(orderItem.ProductData.ContainsKey("variant_sku"));
        Assert.Equal(variant.Sku, orderItem.ProductData["variant_sku"]);
        Assert.True(orderItem.ProductData.ContainsKey("variant_attributes"));
    }

    [Fact]
    public void Create_ShouldCalculateTotalPriceCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var product = CreateTestProduct();
        var quantity = 3;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        // Act
        var orderItemResult = order.AddItem(product, quantity, unitPrice);

        // Assert
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;
        Assert.Equal(unitPrice.Amount * quantity, orderItem.TotalPrice.Amount);
        Assert.Equal(unitPrice.Currency, orderItem.TotalPrice.Currency);
    }

    [Fact]
    public void UpdateQuantity_WithPositiveValue_ShouldUpdateQuantityAndTotalPrice()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var product = CreateTestProduct();
        var initialQuantity = 2;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        var orderItemResult = order.AddItem(product, initialQuantity, unitPrice);
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;

        var initialTotalPrice = orderItem.TotalPrice.Amount;
        Assert.Equal(unitPrice.Amount * initialQuantity, initialTotalPrice);

        var newQuantity = 5;

        // Act
        var updateResult = order.UpdateOrderItemQuantity(orderItem.Id, newQuantity);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newQuantity, orderItem.Quantity);
        Assert.Equal(unitPrice.Amount * newQuantity, orderItem.TotalPrice.Amount);
    }

    [Fact]
    public void UpdateQuantity_WithZeroValue_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var product = CreateTestProduct();
        var initialQuantity = 2;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        var orderItemResult = order.AddItem(product, initialQuantity, unitPrice);
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;

        // Act
        var zeroResult = order.UpdateOrderItemQuantity(orderItem.Id, 0);

        // Assert
        Assert.True(zeroResult.IsFailure);
        Assert.Equal("Order.InvalidQuantity", zeroResult.Error.Code);
        Assert.Equal(initialQuantity, orderItem.Quantity); // Quantity should remain unchanged
    }

    [Fact]
    public void UpdateQuantity_WithNegativeValue_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var product = CreateTestProduct();
        var initialQuantity = 2;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        var orderItemResult = order.AddItem(product, initialQuantity, unitPrice);
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;

        // Act
        var negativeResult = order.UpdateOrderItemQuantity(orderItem.Id, -1);

        // Assert
        Assert.True(negativeResult.IsFailure);
        Assert.Equal("Order.InvalidQuantity", negativeResult.Error.Code);
        Assert.Equal(initialQuantity, orderItem.Quantity); // Quantity should remain unchanged
    }

    [Fact]
    public void ProductDataSnapshot_ShouldBeUnaffectedByProductChanges()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);

        var productName = "Original Product";
        var product = CreateTestProduct(productName);
        var quantity = 1;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        var orderItemResult = order.AddItem(product, quantity, unitPrice);
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;

        // Verify original product name is in snapshot
        Assert.Equal(productName, orderItem.ProductData["name"]);

        // Now change the product
        var newSlugResult = Slug.Create("updated-product");
        Assert.True(newSlugResult.IsSuccess);

        var newPriceResult = Money.FromDollars(70);
        Assert.True(newPriceResult.IsSuccess);

        product.Update(
            "Updated Product Name",
            newSlugResult.Value,
            newPriceResult.Value,
            "New description",
            "NEW-SKU");

        // Act & Assert - Snapshot should remain unchanged
        Assert.Equal(productName, orderItem.ProductData["name"]);
        Assert.NotEqual("Updated Product Name", orderItem.ProductData["name"]);
        Assert.Equal("TEST-SKU", orderItem.ProductData["sku"]);
        Assert.NotEqual("NEW-SKU", orderItem.ProductData["sku"]);
    }
}