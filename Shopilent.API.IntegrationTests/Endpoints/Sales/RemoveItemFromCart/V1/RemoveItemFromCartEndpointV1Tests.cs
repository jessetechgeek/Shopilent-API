using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Sales;

namespace Shopilent.API.IntegrationTests.Endpoints.Sales.RemoveItemFromCart.V1;

public class RemoveItemFromCartEndpointV1Tests : ApiIntegrationTestBase
{
    public RemoveItemFromCartEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task RemoveItemFromCart_WithValidItemId_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 2);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse!.Data.Should().Be("Item successfully removed from cart");
    }

    [Fact]
    public async Task RemoveItemFromCart_WithValidItemId_ShouldRemoveItemFromDatabase()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 3);

        // Verify cart item exists
        await ExecuteDbContextAsync(async context =>
        {
            var item = await context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id);
            item.Should().NotBeNull();
        });

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify cart item no longer exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var item = await context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id);
            item.Should().BeNull();
        });
    }

    [Fact]
    public async Task RemoveItemFromCart_WithMultipleItemsInCart_ShouldRemoveOnlySpecifiedItem()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var product1 = await SeedProductAsync();
        var product2 = await SeedProductAsync();
        var product3 = await SeedProductAsync();

        var cartItem1 = await SeedCartItemAsync(cart.Id, product1.Id, quantity: 2);
        var cartItem2 = await SeedCartItemAsync(cart.Id, product2.Id, quantity: 3);
        var cartItem3 = await SeedCartItemAsync(cart.Id, product3.Id, quantity: 1);

        // Act - Remove only item 2
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem2.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify only item 2 was removed
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().HaveCount(2);
            cartWithItems.Items.Should().Contain(i => i.Id == cartItem1.Id);
            cartWithItems.Items.Should().NotContain(i => i.Id == cartItem2.Id);
            cartWithItems.Items.Should().Contain(i => i.Id == cartItem3.Id);
        });
    }

    [Fact]
    public async Task RemoveItemFromCart_LastItemInCart_ShouldLeaveEmptyCart()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify cart still exists but has no items
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task RemoveItemFromCart_AnonymousCart_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var product = await SeedProductAsync();
        var cart = await SeedAnonymousCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 2);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task RemoveItemFromCart_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        // No cart setup needed for validation test

        // Act
        var response = await DeleteAsync($"v1/cart/items/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Item ID is required.");
    }

    [Fact]
    public async Task RemoveItemFromCart_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        // No cart setup needed for validation test

        // Act
        var response = await DeleteAsync("v1/cart/items/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveItemFromCart_WithNonExistentItemId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentItemId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/cart/items/{nonExistentItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "Cart", "does not exist");
    }

    [Fact]
    public async Task RemoveItemFromCart_AlreadyRemoved_ShouldReturnNotFound()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // Delete the item first time
        var firstDeleteResponse = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstDeleteContent = await firstDeleteResponse.Content.ReadAsStringAsync();
        var firstDeleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(firstDeleteContent, JsonOptions);
        AssertApiSuccess(firstDeleteApiResponse);

        // Act - Try to delete again
        var response = await DeleteAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task RemoveItemFromCart_WithoutAuthentication_ShouldAllowAnonymousCartOperation()
    {
        // Arrange
        ClearAuthenticationHeader();
        var product = await SeedProductAsync();
        var cart = await SeedAnonymousCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert - Should allow anonymous cart operations
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveItemFromCart_AuthenticatedUserOwnCart_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var product = await SeedProductAsync();
        var customerUser = await GetCustomerUserAsync();
        var cart = await SeedCartForUserAsync(customerUser.Id);
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 2);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task RemoveItemFromCart_AuthenticatedUserDifferentUserCart_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a cart for a different user
        var product = await SeedProductAsync();
        var otherUser = await SeedDifferentUserAsync();
        var cart = await SeedCartForUserAsync(otherUser.Id);
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // Act
        var response = await DeleteAsync($"v1/cart/items/{cartItem.Id}");

        // Assert - Should not be able to remove items from another user's cart
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItemFromCart_WithAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var product = await SeedProductAsync();
        var adminUser = await GetAdminUserAsync();
        var cart = await SeedCartForUserAsync(adminUser.Id);
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task RemoveItemFromCart_WithProductVariant_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var variant = await SeedProductVariantAsync(product.Id);
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemWithVariantAsync(cart.Id, product.Id, variant.Id, quantity: 2);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify item removed from database
        await ExecuteDbContextAsync(async context =>
        {
            var item = await context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id);
            item.Should().BeNull();
        });
    }

    [Fact]
    public async Task RemoveItemFromCart_WithMultipleVariantsOfSameProduct_ShouldRemoveOnlySpecifiedVariant()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var product = await SeedProductAsync();
        var variant1 = await SeedProductVariantAsync(product.Id);
        var variant2 = await SeedProductVariantAsync(product.Id);

        var cartItem1 = await SeedCartItemWithVariantAsync(cart.Id, product.Id, variant1.Id, quantity: 2);
        var cartItem2 = await SeedCartItemWithVariantAsync(cart.Id, product.Id, variant2.Id, quantity: 3);

        // Act - Remove only variant 1
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem1.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify only variant 1 was removed
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().HaveCount(1);
            cartWithItems.Items.Should().NotContain(i => i.Id == cartItem1.Id);
            cartWithItems.Items.Should().Contain(i => i.Id == cartItem2.Id && i.VariantId == variant2.Id);
        });
    }

    [Fact]
    public async Task RemoveItemFromCart_WithLargeQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 100);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task RemoveItemFromCart_SequentialDeletes_ShouldHandleGracefully()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var cartItemIds = new List<Guid>();

        for (int i = 0; i < 5; i++)
        {
            var product = await SeedProductAsync();
            var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);
            cartItemIds.Add(cartItem.Id);
        }

        // Act & Assert - Delete items sequentially
        foreach (var cartItemId in cartItemIds)
        {
            var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItemId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
            AssertApiSuccess(apiResponse);
        }

        // Verify all items removed from cart
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task RemoveItemFromCart_ConcurrentDeletesFromDifferentCarts_ShouldSucceed()
    {
        // Arrange - Create multiple carts with items
        var cartItems = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var cart = await SeedCartAsync();
            var product = await SeedProductAsync();
            var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);
            cartItems.Add(cartItem.Id);
        }

        // Act - Delete items concurrently from different carts (no conflict)
        var tasks = cartItems
            .Select(itemId => DeleteApiResponseAsync($"v1/cart/items/{itemId}"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    #endregion

    #region Integration with Other Endpoints Tests

    [Fact]
    public async Task RemoveItemFromCart_ThenTryToRemoveAgain_ShouldReturnNotFound()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // First delete
        var firstDeleteResponse = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstDeleteContent = await firstDeleteResponse.Content.ReadAsStringAsync();
        var firstDeleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(firstDeleteContent, JsonOptions);
        AssertApiSuccess(firstDeleteApiResponse);

        // Verify item no longer exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var item = await context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id);
            item.Should().BeNull("Cart item should not exist after deletion");
        });

        // Act - Try to delete again
        var secondDeleteResponse = await DeleteAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItemFromCart_ThenVerifyCartState_ShouldReflectRemoval()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var product1 = await SeedProductAsync();
        var product2 = await SeedProductAsync();

        var cartItem1 = await SeedCartItemAsync(cart.Id, product1.Id, quantity: 2);
        var cartItem2 = await SeedCartItemAsync(cart.Id, product2.Id, quantity: 3);

        // Act - Remove first item
        var deleteResponse = await DeleteApiResponseAsync($"v1/cart/items/{cartItem1.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify cart state in database
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().HaveCount(1);
            cartWithItems.Items.Should().Contain(i => i.Id == cartItem2.Id);
            cartWithItems.Items.Should().NotContain(i => i.Id == cartItem1.Id);
        });
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task RemoveItemFromCart_SuccessResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeTrue();
        apiResponse.Data.Should().NotBeNullOrEmpty();
        apiResponse.Data.Should().Be("Item successfully removed from cart");
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RemoveItemFromCart_ErrorResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var nonExistentItemId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/cart/items/{nonExistentItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // The response should be in ApiResponse format
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeFalse();
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(404);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task RemoveItemFromCart_WithMinimumQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 1);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task RemoveItemFromCart_WithMaximumQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 100);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Cart State Tests

    [Fact]
    public async Task RemoveItemFromCart_FromCartWithSingleItem_ShouldLeaveEmptyCart()
    {
        // Arrange
        var product = await SeedProductAsync();
        var cart = await SeedCartAsync();
        var cartItem = await SeedCartItemAsync(cart.Id, product.Id, quantity: 5);

        // Act
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify cart exists but is empty
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task RemoveItemFromCart_FromCartWithMultipleItems_ShouldKeepOtherItems()
    {
        // Arrange
        var cart = await SeedCartAsync();
        var product1 = await SeedProductAsync();
        var product2 = await SeedProductAsync();
        var product3 = await SeedProductAsync();

        var cartItem1 = await SeedCartItemAsync(cart.Id, product1.Id, quantity: 1);
        var cartItem2 = await SeedCartItemAsync(cart.Id, product2.Id, quantity: 2);
        var cartItem3 = await SeedCartItemAsync(cart.Id, product3.Id, quantity: 3);

        // Act - Remove middle item
        var response = await DeleteApiResponseAsync($"v1/cart/items/{cartItem2.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify other items still exist
        await ExecuteDbContextAsync(async context =>
        {
            var cartWithItems = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            cartWithItems.Should().NotBeNull();
            cartWithItems!.Items.Should().HaveCount(2);
            cartWithItems.Items.Should().Contain(i => i.Id == cartItem1.Id && i.Quantity == 1);
            cartWithItems.Items.Should().Contain(i => i.Id == cartItem3.Id && i.Quantity == 3);
            cartWithItems.Items.Should().NotContain(i => i.Id == cartItem2.Id);
        });
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

    private async Task<Cart> SeedCartForUserAsync(Guid userId)
    {
        return await TestDbSeeder.SeedCartForUserAsync(ExecuteDbContextAsync, userId);
    }

    private async Task<CartItem> SeedCartItemAsync(Guid cartId, Guid productId, int quantity)
    {
        return await TestDbSeeder.SeedCartItemAsync(ExecuteDbContextAsync, cartId, productId, quantity);
    }

    private async Task<CartItem> SeedCartItemWithVariantAsync(Guid cartId, Guid productId, Guid variantId, int quantity)
    {
        return await TestDbSeeder.SeedCartItemWithVariantAsync(ExecuteDbContextAsync, cartId, productId, variantId, quantity);
    }

    private async Task<User> GetCustomerUserAsync()
    {
        return await TestDbSeeder.GetCustomerUserAsync(ExecuteDbContextAsync);
    }

    private async Task<User> GetAdminUserAsync()
    {
        return await TestDbSeeder.GetAdminUserAsync(ExecuteDbContextAsync);
    }

    private async Task<User> SeedDifferentUserAsync()
    {
        return await TestDbSeeder.SeedUserAsync(ExecuteDbContextAsync);
    }

    #endregion
}
