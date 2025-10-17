using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Sales.Commands.AddItemToCart.V1;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Sales;

namespace Shopilent.API.IntegrationTests.Endpoints.Sales.AddItemToCart.V1;

public class AddItemToCartEndpointV1Tests : ApiIntegrationTestBase
{
    public AddItemToCartEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task AddItemToCart_WithValidDataAndNewCart_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id, quantity: 2);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.CartId.Should().NotBeEmpty();
        response.Data.CartItemId.Should().NotBeEmpty();
        response.Data.ProductId.Should().Be(product.Id);
        response.Data.Quantity.Should().Be(2);
        response.Data.VariantId.Should().BeNull();
        response.Data.Message.Should().Be("Item added to cart successfully");
    }

    [Fact]
    public async Task AddItemToCart_WithValidData_ShouldCreateCartAndItemInDatabase()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id, quantity: 3);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);

        // Verify cart and cart item exist in database
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == response!.Data.CartId);

            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCount(1);

            var cartItem = cart.Items.First();
            cartItem.Id.Should().Be(response.Data.CartItemId);
            cartItem.ProductId.Should().Be(product.Id);
            cartItem.Quantity.Should().Be(3);
            cartItem.VariantId.Should().BeNull();
        });
    }

    [Fact]
    public async Task AddItemToCart_WithExistingCartId_ShouldAddToExistingCart()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(
            cartId: cart.Id,
            productId: product.Id,
            quantity: 1);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.CartId.Should().Be(cart.Id);
        response.Data.ProductId.Should().Be(product.Id);
        response.Data.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task AddItemToCart_WithProductVariant_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var variant = await SeedProductVariantAsync(product.Id);
        var request = CartTestDataV1.Creation.CreateRequestWithVariant(
            productId: product.Id,
            variantId: variant.Id,
            quantity: 2);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.ProductId.Should().Be(product.Id);
        response.Data.VariantId.Should().Be(variant.Id);
        response.Data.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task AddItemToCart_AuthenticatedUser_ShouldAssignCartToUser()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);

        // Verify cart is assigned to user in database
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .FirstOrDefaultAsync(c => c.Id == response!.Data.CartId);

            cart.Should().NotBeNull();
            cart!.UserId.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task AddItemToCart_AnonymousUser_ShouldCreateAnonymousCart()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no authentication
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);

        // Verify cart is anonymous (no user) in database
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .FirstOrDefaultAsync(c => c.Id == response!.Data.CartId);

            cart.Should().NotBeNull();
            cart!.UserId.Should().BeNull();
        });
    }

    [Fact]
    public async Task AddItemToCart_SameProductTwice_ShouldUpdateQuantity()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();

        // First add
        var firstRequest = CartTestDataV1.Creation.CreateValidRequest(
            cartId: cart.Id,
            productId: product.Id,
            quantity: 2);

        var firstResponse = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", firstRequest);
        AssertApiSuccess(firstResponse);

        // Second add of same product
        var secondRequest = CartTestDataV1.Creation.CreateValidRequest(
            cartId: cart.Id,
            productId: product.Id,
            quantity: 3);

        // Act
        var secondResponse =
            await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", secondRequest);

        // Assert
        AssertApiSuccess(secondResponse);

        // Verify quantity updated in database
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            var items = cartWithItems!.Items.Where(i => i.ProductId == product.Id).ToList();
            items.Should().HaveCount(1); // Should be single item with updated quantity
            items[0].Quantity.Should().Be(5); // 2 + 3 = 5
        });
    }

    #endregion

    #region Validation Tests - ProductId

    [Fact]
    public async Task AddItemToCart_WithEmptyProductId_ShouldReturnValidationError()
    {
        // Arrange
        var request = CartTestDataV1.Validation.CreateRequestWithEmptyProductId();

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product ID is required.");
    }

    [Fact]
    public async Task AddItemToCart_WithNonExistentProductId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var request = CartTestDataV1.Validation.CreateRequestWithNonExistentProductId();

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Product", "not found", "does not exist");
    }

    #endregion

    #region Validation Tests - Quantity

    [Fact]
    public async Task AddItemToCart_WithZeroQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Validation.CreateRequestWithZeroQuantity(productId: product.Id);

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Quantity must be greater than zero.");
    }

    [Fact]
    public async Task AddItemToCart_WithNegativeQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Validation.CreateRequestWithNegativeQuantity(productId: product.Id);

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Quantity must be greater than zero.");
    }

    [Fact]
    public async Task AddItemToCart_WithExcessiveQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Validation.CreateRequestWithExcessiveQuantity(productId: product.Id);

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Quantity cannot exceed 100 items.");
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(200)]
    public async Task AddItemToCart_WithInvalidQuantity_ShouldReturnValidationError(int invalidQuantity)
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.QuantityScenarios.CreateRequestWithQuantity(
            null,
            product.Id,
            invalidQuantity);

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Validation Tests - VariantId

    [Fact]
    public async Task AddItemToCart_WithNonExistentVariantId_ShouldReturnNotFound()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Validation.CreateRequestWithNonExistentVariantId(productId: product.Id);

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("variant", "not found", "does not exist");
    }

    #endregion

    #region Validation Tests - CartId

    [Fact]
    public async Task AddItemToCart_WithNonExistentCartId_ShouldReturnNotFound()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Validation.CreateRequestWithNonExistentCartId(productId: product.Id);

        // Act
        var response = await PostAsync("v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Cart", "not found", "does not exist");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task AddItemToCart_WithMinimumValidQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.BoundaryTests.CreateRequestWithMinimumValidQuantity(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task AddItemToCart_WithMaximumValidQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.BoundaryTests.CreateRequestWithMaximumValidQuantity(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(100);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task AddItemToCart_WithVariousValidQuantities_ShouldReturnSuccess(int quantity)
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.QuantityScenarios.CreateRequestWithQuantity(null, product.Id, quantity);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(quantity);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task AddItemToCart_WithNullVariant_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.EdgeCases.CreateRequestWithNullVariant(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.VariantId.Should().BeNull();
    }

    [Fact]
    public async Task AddItemToCart_MultipleProducts_ShouldAddAllToCart()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var product1 = await SeedProductAsync();
        var product2 = await SeedProductAsync();
        var product3 = await SeedProductAsync();

        // Act - Add three different products
        var request1 = CartTestDataV1.Creation.CreateValidRequest(cartId: cart.Id, productId: product1.Id, quantity: 2);
        var request2 = CartTestDataV1.Creation.CreateValidRequest(cartId: cart.Id, productId: product2.Id, quantity: 3);
        var request3 = CartTestDataV1.Creation.CreateValidRequest(cartId: cart.Id, productId: product3.Id, quantity: 1);

        var response1 = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request1);
        var response2 = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request2);
        var response3 = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request3);

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);
        AssertApiSuccess(response3);

        // Verify all items in database
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().HaveCount(3);
            cartWithItems.Items.Should().Contain(i => i.ProductId == product1.Id && i.Quantity == 2);
            cartWithItems.Items.Should().Contain(i => i.ProductId == product2.Id && i.Quantity == 3);
            cartWithItems.Items.Should().Contain(i => i.ProductId == product3.Id && i.Quantity == 1);
        });
    }

    [Fact]
    public async Task AddItemToCart_DifferentVariantsOfSameProduct_ShouldAddSeparately()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var product = await SeedProductAsync();
        var variant1 = await SeedProductVariantAsync(product.Id);
        var variant2 = await SeedProductVariantAsync(product.Id);

        // Act - Add same product with different variants
        var request1 = CartTestDataV1.Creation.CreateRequestWithVariant(
            cartId: cart.Id, productId: product.Id, variantId: variant1.Id, quantity: 2);
        var request2 = CartTestDataV1.Creation.CreateRequestWithVariant(
            cartId: cart.Id, productId: product.Id, variantId: variant2.Id, quantity: 3);

        var response1 = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request1);
        var response2 = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request2);

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Verify separate cart items in database
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().HaveCount(2);
            cartWithItems.Items.Should().Contain(i => i.VariantId == variant1.Id && i.Quantity == 2);
            cartWithItems.Items.Should().Contain(i => i.VariantId == variant2.Id && i.Quantity == 3);
        });
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task AddItemToCart_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert - Should allow anonymous cart creation
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task AddItemToCart_WithCustomerAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task AddItemToCart_WithAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task AddItemToCart_SequentialRequests_ShouldAddAllItemsToCart()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var products = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var product = await SeedProductAsync();
            products.Add(product.Id);
        }

        // Act - Add items sequentially to avoid concurrency conflicts
        var responses = new List<AddItemToCartResponseV1>();
        foreach (var productId in products)
        {
            var request = CartTestDataV1.Creation.CreateValidRequest(cartId: cart.Id, productId: productId, quantity: 1);
            var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);
            AssertApiSuccess(response);
            responses.Add(response!.Data);
        }

        // Assert
        responses.Should().HaveCount(5);
        responses.Select(r => r.CartItemId).Should().OnlyHaveUniqueItems();
        responses.Should().AllSatisfy(r => r.CartId.Should().Be(cart.Id));

        // Verify all items in database
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().HaveCount(5);
        });
    }

    [Fact]
    public async Task AddItemToCart_ConcurrentRequestsToDifferentCarts_ShouldSucceed()
    {
        // Arrange - Create multiple carts and products
        var cartsAndProducts = new List<(Guid CartId, Guid ProductId)>();
        for (int i = 0; i < 5; i++)
        {
            var cart = await SeedCartAsync();
            var product = await SeedProductAsync();
            cartsAndProducts.Add((cart.Id, product.Id));
        }

        // Act - Add items concurrently to different carts (no conflict)
        var tasks = cartsAndProducts
            .Select(cp => CartTestDataV1.Creation.CreateValidRequest(cartId: cp.CartId, productId: cp.ProductId, quantity: 1))
            .Select(request => PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.CartItemId).Should().OnlyHaveUniqueItems();
        responses.Select(r => r!.Data.CartId).Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region Cart Ownership Tests

    [Fact]
    public async Task AddItemToCart_AuthenticatedUserWithAnonymousCart_ShouldAssignCartToUser()
    {
        // Arrange
        var anonymousCart = await SeedAnonymousCartAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var product = await SeedProductAsync();

        var request = CartTestDataV1.Creation.CreateValidRequest(
            cartId: anonymousCart.Id,
            productId: product.Id);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);

        // Verify cart is now assigned to user
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts.FirstOrDefaultAsync(c => c.Id == anonymousCart.Id);
            cart.Should().NotBeNull();
            cart!.UserId.Should().NotBeNull();
        });
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task AddItemToCart_ValidRequest_ShouldReturnCompleteResponse()
    {
        // Arrange
        var product = await SeedProductAsync();
        var request = CartTestDataV1.Creation.CreateValidRequest(productId: product.Id, quantity: 5);

        // Act
        var response = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.CartId.Should().NotBeEmpty();
        response.Data.CartItemId.Should().NotBeEmpty();
        response.Data.ProductId.Should().Be(product.Id);
        response.Data.Quantity.Should().Be(5);
        response.Data.Message.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Helper Methods

    private async Task<Product> SeedProductAsync()
    {
        return await TestDbSeeder.SeedProductAsync(ExecuteDbContextAsync);
    }

    private async Task<ProductVariant> SeedProductVariantAsync(Guid productId)
    {
        return await TestDbSeeder.SeedProductVariantAsync(ExecuteDbContextAsync, productId);
    }

    private async Task<Cart> SeedCartAsync()
    {
        return await TestDbSeeder.SeedAnonymousCartAsync(ExecuteDbContextAsync);
    }

    private async Task<Cart> SeedAnonymousCartAsync()
    {
        return await TestDbSeeder.SeedAnonymousCartAsync(ExecuteDbContextAsync);
    }

    #endregion
}
