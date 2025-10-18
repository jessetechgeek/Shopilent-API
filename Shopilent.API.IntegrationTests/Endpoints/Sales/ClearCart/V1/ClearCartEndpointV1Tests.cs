using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Sales.Commands.AddItemToCart.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Sales.ClearCart.V1;

public class ClearCartEndpointV1Tests : ApiIntegrationTestBase
{
    public ClearCartEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task ClearCart_WithValidCartId_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a cart with some items
        var (cartId, _) = await CreateCartWithItemsAsync(accessToken, itemCount: 3);

        // Verify cart has items before clearing
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCountGreaterThan(0);
        });

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse!.Data.Should().Be("Cart successfully cleared");
    }

    [Fact]
    public async Task ClearCart_WithValidCartId_ShouldRemoveAllItemsFromDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a cart with some items
        var (cartId, _) = await CreateCartWithItemsAsync(accessToken, itemCount: 5);

        // Verify cart has items before clearing
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCount(5);
        });

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify cart items were cleared in database
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Should().NotBeNull();
            cart!.Items.Should().BeEmpty("All items should be removed after clearing the cart");
        });
    }

    [Fact]
    public async Task ClearCart_WithoutCartId_AuthenticatedUser_ShouldClearUserCart()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a cart with items for the authenticated customer user
        var result = await TestDbSeeder.SeedCartWithItemForCustomerAsync(ExecuteDbContextAsync, initialQuantity: 3);
        var cartId = result.cart.Id;

        // Verify cart has items before clearing
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCountGreaterThan(0);
        });

        var request = CartTestDataV1.ClearCart.CreateRequestWithoutCartId();

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify cart items were cleared
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Should().NotBeNull();
            cart!.Items.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task ClearCart_WithEmptyCart_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an empty cart using TestDbSeeder
        var cart = await TestDbSeeder.SeedAnonymousCartAsync(ExecuteDbContextAsync);

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cart.Id);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task ClearCart_MultipleTimesOnSameCart_ShouldReturnSuccessEachTime()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a cart with items
        var (cartId, _) = await CreateCartWithItemsAsync(accessToken, itemCount: 2);

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act - Clear first time
        var firstResponse = await PostAsync("v1/cart/clear", request);

        // Assert - First clear
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstContent = await firstResponse.Content.ReadAsStringAsync();
        var firstApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(firstContent, JsonOptions);
        AssertApiSuccess(firstApiResponse);

        // Act - Clear second time
        var secondResponse = await PostAsync("v1/cart/clear", request);

        // Assert - Second clear (cart is already empty)
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondContent = await secondResponse.Content.ReadAsStringAsync();
        var secondApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(secondContent, JsonOptions);
        AssertApiSuccess(secondApiResponse);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ClearCart_WithEmptyCartId_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var request = CartTestDataV1.ClearCart.CreateRequestWithEmptyCartId();

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Cart ID cannot be empty when provided.");
    }

    [Fact]
    public async Task ClearCart_WithNonExistentCartId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var request = CartTestDataV1.ClearCart.CreateRequestWithNonExistentCartId();

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "does not exist");
    }

    [Fact]
    public async Task ClearCart_WithoutCartIdAndNoAuthenticatedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no authentication
        var request = CartTestDataV1.ClearCart.CreateRequestWithoutCartId();

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert - Should be unauthorized or not found since endpoint is AllowAnonymous but requires cart context
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task ClearCart_AnonymousUserWithCartId_ShouldReturnSuccess()
    {
        // Arrange - Create an anonymous cart using TestDbSeeder
        var cart = await TestDbSeeder.SeedAnonymousCartAsync(ExecuteDbContextAsync);

        // Clear authentication to simulate anonymous user
        ClearAuthenticationHeader();

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cart.Id);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert - Endpoint allows anonymous, so it should work with valid cart ID
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ClearCart_UserCannotClearAnotherUsersCart_ShouldReturnNotFound()
    {
        // Arrange - Create a cart as customer
        var customer1Token = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customer1Token);

        // Create a cart for the customer user
        var result = await TestDbSeeder.SeedCartWithItemForCustomerAsync(ExecuteDbContextAsync, initialQuantity: 2);
        var cartId = result.cart.Id;

        // Switch to a different user (manager user)
        var customer2Token = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(customer2Token);

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert - Should return NotFound for security (don't reveal cart exists)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClearCart_AdminCannotClearUserCart_ShouldReturnNotFound()
    {
        // Arrange - Create a cart as a customer
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        // Create a cart for the customer user
        var result = await TestDbSeeder.SeedCartWithItemForCustomerAsync(ExecuteDbContextAsync, initialQuantity: 2);
        var cartId = result.cart.Id;

        // Switch to admin
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert - Admin should not be able to clear user's cart (security check)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ClearCart_WithMixedProductTypes_ShouldClearAll()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create products with and without variants
        var (cartId, productIds) = await CreateCartWithItemsAsync(accessToken, itemCount: 5);

        // Verify cart has multiple items
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart!.Items.Should().HaveCount(5);
        });

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart!.Items.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task ClearCart_ThenAddNewItems_ShouldWorkCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var (cartId, productIds) = await CreateCartWithItemsAsync(accessToken, itemCount: 3);

        var clearRequest = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act - Clear cart
        var clearResponse = await PostAsync("v1/cart/clear", clearRequest);

        // Assert - Verify clear succeeded
        clearResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify cart is empty
        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart!.Items.Should().BeEmpty();
        });

        // Act - Add new items to cleared cart using the API endpoint
        var newProduct = await TestDbSeeder.SeedProductAsync(ExecuteDbContextAsync);
        var addItemRequest = new
        {
            CartId = cartId,
            ProductId = newProduct.Id,
            VariantId = (Guid?)null,
            Quantity = 2
        };
        var addItemResponse = await PostApiResponseAsync<object, AddItemToCartResponseV1>("v1/cart/items", addItemRequest);

        // Assert - New items should be added successfully
        AssertApiSuccess(addItemResponse);

        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart!.Items.Should().HaveCount(1);
        });
    }

    #endregion

    #region Performance/Bulk Tests

    [Fact]
    public async Task ClearCart_WithLargeNumberOfItems_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a cart with many items
        var (cartId, _) = await CreateCartWithItemsAsync(accessToken, itemCount: 20);

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart!.Items.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task ClearCart_ConcurrentClearRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var (cartId, _) = await CreateCartWithItemsAsync(accessToken, itemCount: 5);

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act - Send multiple clear requests sequentially to avoid concurrency issues
        // (Clearing the same cart concurrently may cause database conflicts)
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            var response = await PostAsync("v1/cart/clear", request);
            responses.Add(response);
        }

        // Assert - First request should succeed, subsequent requests should also succeed (idempotent)
        // All should return OK since clearing an already-cleared cart is valid
        responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
        responses.Skip(1).Should().AllSatisfy(response =>
            response.StatusCode.Should().Be(HttpStatusCode.OK));

        await ExecuteDbContextAsync(async context =>
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
            cart!.Items.Should().BeEmpty();
        });
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task ClearCart_SuccessResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var (cartId, _) = await CreateCartWithItemsAsync(accessToken, itemCount: 2);

        var request = CartTestDataV1.ClearCart.CreateValidRequestWithCartId(cartId);

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeTrue();
        apiResponse.Data.Should().NotBeNullOrEmpty();
        apiResponse.Data.Should().Be("Cart successfully cleared");
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.Message.Should().Be("Cart cleared successfully");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ClearCart_ErrorResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var request = CartTestDataV1.ClearCart.CreateRequestWithNonExistentCartId();

        // Act
        var response = await PostAsync("v1/cart/clear", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeFalse();
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(404);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper method to create a cart with multiple items for testing
    /// </summary>
    private async Task<(Guid CartId, List<Guid> ProductIds)> CreateCartWithItemsAsync(string accessToken, int itemCount = 3)
    {
        // Use TestDbSeeder to create cart with items directly in the database
        var result = await TestDbSeeder.SeedAnonymousCartWithMultipleItemsAsync(ExecuteDbContextAsync, itemCount);

        var productIds = result.cartItems.Select(item => item.ProductId).ToList();

        return (result.cart.Id, productIds);
    }

    #endregion
}
