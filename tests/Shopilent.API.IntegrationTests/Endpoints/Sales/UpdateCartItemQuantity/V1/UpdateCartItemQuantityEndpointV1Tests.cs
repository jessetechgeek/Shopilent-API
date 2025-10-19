using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Sales.Commands.UpdateCartItemQuantity.V1;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Sales;

namespace Shopilent.API.IntegrationTests.Endpoints.Sales.UpdateCartItemQuantity.V1;

public class UpdateCartItemQuantityEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateCartItemQuantityEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateCartItemQuantity_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 10);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.CartItemId.Should().Be(cartItem.Id);
        response.Data.Quantity.Should().Be(10);
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithValidData_ShouldUpdateQuantityInDatabase()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 15);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify quantity updated in database
        await ExecuteDbContextAsync(async context =>
        {
            var updatedCart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            updatedCart.Should().NotBeNull();
            var updatedItem = updatedCart!.Items.FirstOrDefault(i => i.Id == cartItem.Id);
            updatedItem.Should().NotBeNull();
            updatedItem!.Quantity.Should().Be(15);
        });
    }

    [Fact]
    public async Task UpdateCartItemQuantity_IncreasingQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync(initialQuantity: 5);
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 20);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(20);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_DecreasingQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync(initialQuantity: 50);
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 10);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(10);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_ToMinimumQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync(initialQuantity: 10);
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithMinimumValidQuantity();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_AuthenticatedUserWithOwnCart_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var (cart, cartItem) = await SeedCartWithItemForAuthenticatedUserAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 8);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(8);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_AnonymousCart_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 7);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(7);
    }

    #endregion

    #region Validation Tests - Quantity

    [Fact]
    public async Task UpdateCartItemQuantity_WithZeroQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithZeroQuantity();

        // Act
        var response = await PutAsync($"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Quantity must be greater than 0");
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithNegativeQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithNegativeQuantity();

        // Act
        var response = await PutAsync($"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Quantity must be greater than 0");
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithExcessiveQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithExcessiveQuantity();

        // Act
        var response = await PutAsync($"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Quantity cannot exceed 999");
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(2000)]
    public async Task UpdateCartItemQuantity_WithInvalidQuantity_ShouldReturnValidationError(int invalidQuantity)
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity(invalidQuantity);

        // Act
        var response = await PutAsync($"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Validation Tests - Cart Item ID

    [Fact]
    public async Task UpdateCartItemQuantity_WithNonExistentCartItemId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentItemId = Guid.NewGuid();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/cart/items/{nonExistentItemId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "Cart");
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithMalformedGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var malformedId = "not-a-valid-guid";
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/cart/items/{malformedId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/cart/items/{emptyGuid}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task UpdateCartItemQuantity_WithMinimumValidQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithMinimumValidQuantity();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithMaximumValidQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithMaximumValidQuantity();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(999);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithNearMaximumValidQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithNearMaximumQuantity();

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(998);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithJustAboveMaximumQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithJustAboveMaximumQuantity();

        // Act
        var response = await PutAsync($"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("999");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(999)]
    public async Task UpdateCartItemQuantity_WithVariousValidQuantities_ShouldReturnSuccess(int quantity)
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity(quantity);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(quantity);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateCartItemQuantity_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange - Anonymous cart (no authentication required)
        ClearAuthenticationHeader();
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 6);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert - Should allow anonymous cart updates
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(6);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithCustomerAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var (cart, cartItem) = await SeedCartWithItemForAuthenticatedUserAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 12);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(12);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var (cart, cartItem) = await SeedCartWithItemForAdminUserAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 9);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(9);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateCartItemQuantity_SameQuantityAsExisting_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync(initialQuantity: 5);
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity(5);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithProductVariant_ShouldReturnSuccess()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAndVariantAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 15);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(15);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithMultipleItemsInCart_ShouldUpdateOnlyTargetItem()
    {
        // Arrange
        var (cart, cartItems) = await SeedCartWithMultipleItemsAsync(itemCount: 3);
        var targetItem = cartItems[1]; // Update the second item
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 25);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{targetItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.CartItemId.Should().Be(targetItem.Id);
        response.Data.Quantity.Should().Be(25);

        // Verify only the target item was updated
        await ExecuteDbContextAsync(async context =>
        {
            var updatedCart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            updatedCart.Should().NotBeNull();
            updatedCart!.Items.Should().HaveCount(3);

            var updatedItem = updatedCart.Items.FirstOrDefault(i => i.Id == targetItem.Id);
            updatedItem.Should().NotBeNull();
            updatedItem!.Quantity.Should().Be(25);

            // Verify other items remain unchanged
            var otherItems = updatedCart.Items.Where(i => i.Id != targetItem.Id);
            otherItems.Should().AllSatisfy(item => item.Quantity.Should().Be(5)); // Initial quantity
        });
    }

    [Fact]
    public async Task UpdateCartItemQuantity_SequentialUpdates_ShouldApplyAllChanges()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync(initialQuantity: 1);

        // Act - Multiple sequential updates
        var request1 = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity(5);
        var response1 = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", request1);

        var request2 = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity(10);
        var response2 = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", request2);

        var request3 = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity(3);
        var response3 = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", request3);

        // Assert
        AssertApiSuccess(response1);
        response1!.Data.Quantity.Should().Be(5);

        AssertApiSuccess(response2);
        response2!.Data.Quantity.Should().Be(10);

        AssertApiSuccess(response3);
        response3!.Data.Quantity.Should().Be(3);

        // Verify final quantity in database
        await ExecuteDbContextAsync(async context =>
        {
            var updatedCart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var finalItem = updatedCart!.Items.FirstOrDefault(i => i.Id == cartItem.Id);
            finalItem!.Quantity.Should().Be(3);
        });
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateCartItemQuantity_ConcurrentUpdatesOnDifferentItems_ShouldSucceed()
    {
        // Arrange - Create cart with multiple items
        var (cart, cartItems) = await SeedCartWithMultipleItemsAsync(itemCount: 5);

        // Create concurrent update tasks for different items
        var tasks = cartItems.Select((item, index) =>
        {
            var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity((index + 1) * 10);
            return PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
                $"v1/cart/items/{item.Id}", updateRequest);
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.CartItemId).Should().OnlyHaveUniqueItems();

        // Verify each item has correct quantity
        for (int i = 0; i < responses.Length; i++)
        {
            responses[i]!.Data.Quantity.Should().Be((i + 1) * 10);
        }
    }

    [Fact]
    public async Task UpdateCartItemQuantity_MultipleSequentialUpdates_ShouldHandleGracefully()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync(initialQuantity: 1);

        // Act - Perform 10 sequential updates
        var quantities = new[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
        foreach (var quantity in quantities)
        {
            var updateRequest = CartTestDataV1.UpdateQuantity.CreateRequestWithQuantity(quantity);
            var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
                $"v1/cart/items/{cartItem.Id}", updateRequest);

            AssertApiSuccess(response);
            response!.Data.Quantity.Should().Be(quantity);
        }

        // Verify final state
        await ExecuteDbContextAsync(async context =>
        {
            var updatedCart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var finalItem = updatedCart!.Items.FirstOrDefault(i => i.Id == cartItem.Id);
            finalItem!.Quantity.Should().Be(20);
        });
    }

    #endregion

    #region Cart Ownership Tests

    [Fact]
    public async Task UpdateCartItemQuantity_AuthenticatedUserUpdatingOwnCart_ShouldSucceed()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var (cart, cartItem) = await SeedCartWithItemForAuthenticatedUserAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 11);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(11);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_AnonymousCartUpdate_ShouldSucceed()
    {
        // Arrange
        ClearAuthenticationHeader();
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 14);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Quantity.Should().Be(14);

        // Verify cart remains anonymous
        await ExecuteDbContextAsync(async context =>
        {
            var updatedCart = await context.Carts.FirstOrDefaultAsync(c => c.Id == cart.Id);
            updatedCart!.UserId.Should().BeNull();
        });
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task UpdateCartItemQuantity_ValidRequest_ShouldReturnCompleteResponse()
    {
        // Arrange
        var (cart, cartItem) = await SeedCartWithItemAsync();
        var updateRequest = CartTestDataV1.UpdateQuantity.CreateValidRequest(quantity: 17);

        // Act
        var response = await PutApiResponseAsync<object, UpdateCartItemQuantityResponseV1>(
            $"v1/cart/items/{cartItem.Id}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.CartItemId.Should().Be(cartItem.Id);
        response.Data.CartItemId.Should().NotBeEmpty();
        response.Data.Quantity.Should().Be(17);
        response.Data.UpdatedAt.Should().NotBe(default);
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Helper Methods

    private async Task<(Cart cart, CartItem cartItem)> SeedCartWithItemAsync(int initialQuantity = 5)
    {
        return await TestDbSeeder.SeedAnonymousCartWithItemAsync(ExecuteDbContextAsync, initialQuantity);
    }

    private async Task<(Cart cart, CartItem cartItem)> SeedCartWithItemAndVariantAsync(int initialQuantity = 5)
    {
        return await TestDbSeeder.SeedAnonymousCartWithItemAndVariantAsync(ExecuteDbContextAsync, initialQuantity);
    }

    private async Task<(Cart cart, List<CartItem> cartItems)> SeedCartWithMultipleItemsAsync(int itemCount = 3,
        int initialQuantity = 5)
    {
        return await TestDbSeeder.SeedAnonymousCartWithMultipleItemsAsync(ExecuteDbContextAsync, itemCount, initialQuantity);
    }

    private async Task<(Cart cart, CartItem cartItem)> SeedCartWithItemForAuthenticatedUserAsync(
        int initialQuantity = 5)
    {
        return await TestDbSeeder.SeedCartWithItemForCustomerAsync(ExecuteDbContextAsync, initialQuantity);
    }

    private async Task<(Cart cart, CartItem cartItem)> SeedCartWithItemForAdminUserAsync(
        int initialQuantity = 5)
    {
        return await TestDbSeeder.SeedCartWithItemForAdminAsync(ExecuteDbContextAsync, initialQuantity);
    }

    #endregion
}
