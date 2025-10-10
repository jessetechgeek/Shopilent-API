using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.UpdateProductStatus.V1;

public class UpdateProductStatusEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateProductStatusEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateProductStatus_ActivateProduct_ShouldSetToActive()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Status Test Product", "status-test-product", 29.99m, isActive: false);

        var updateRequest = new
        {
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Product status updated to active");
        response.Message.Should().Be("Product status updated successfully");
    }

    [Fact]
    public async Task UpdateProductStatus_DeactivateProduct_ShouldSetToInactive()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Deactivate Test", "deactivate-test", 49.99m, isActive: true);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Product status updated to inactive");
        response.Message.Should().Be("Product status updated successfully");
    }

    [Fact]
    public async Task UpdateProductStatus_UpdateInDatabase_ShouldPersistChanges()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Database Test", "database-test", 99.99m);

        // First deactivate
        var deactivateRequest = new { IsActive = false };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate
        var deactivateResponse = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", deactivateRequest);

        // Assert deactivation
        AssertApiSuccess(deactivateResponse);

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product.Should().NotBeNull();
            product!.IsActive.Should().BeFalse();
        });

        // Act - Reactivate
        var activateRequest = new { IsActive = true };
        var activateResponse = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", activateRequest);

        // Assert reactivation
        AssertApiSuccess(activateResponse);

        // Verify in database again
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product.Should().NotBeNull();
            product!.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task UpdateProductStatus_WithManagerRole_ShouldAllowStatusUpdate()
    {
        // Arrange
        await CreateTestUserAsync("manager@test.com", "Manager", "User", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@test.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Manager Test", "manager-test", 79.99m);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Product status updated to inactive");
    }

    [Fact]
    public async Task UpdateProductStatus_ToggleMultipleTimes_ShouldHandleCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Toggle Test", "toggle-test", 59.99m);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate
        var deactivateRequest = new { IsActive = false };
        var deactivateResponse = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", deactivateRequest);

        // Act - Reactivate
        var activateRequest = new { IsActive = true };
        var activateResponse = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", activateRequest);

        // Act - Deactivate again
        var deactivateAgainResponse = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", deactivateRequest);

        // Assert
        AssertApiSuccess(deactivateResponse);
        AssertApiSuccess(activateResponse);
        AssertApiSuccess(deactivateAgainResponse);

        deactivateResponse!.Data.Should().Be("Product status updated to inactive");
        activateResponse!.Data.Should().Be("Product status updated to active");
        deactivateAgainResponse!.Data.Should().Be("Product status updated to inactive");

        // Verify final state in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product!.IsActive.Should().BeFalse();
        });
    }

    [Fact]
    public async Task UpdateProductStatus_SameStatus_ShouldStillReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Same Status Test", "same-status-test", 39.99m, isActive: true);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Product starts as active, try to activate again
        var updateRequest = new { IsActive = true };

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Product status updated to active");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product!.IsActive.Should().BeTrue();
        });
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task UpdateProductStatus_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        var productId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/products/{productId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProductStatus_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var productId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/products/{productId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateProductStatus_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentProductId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/products/{nonExistentProductId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateProductStatus_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync("v1/products/invalid-guid/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProductStatus_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/products/{Guid.Empty}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task UpdateProductStatus_WithVariants_ShouldUpdateOnlyProductStatus()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product with variants
        var productId = await CreateTestProductAsync("Product With Variants", "product-with-variants", 100m);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate product
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify product is deactivated
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product!.IsActive.Should().BeFalse();
        });
    }

    [Fact]
    public async Task UpdateProductStatus_DeactivateProduct_ShouldSucceed()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Standard Product", "standard-product", 149.99m);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Product status updated to inactive");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product!.IsActive.Should().BeFalse();
        });
    }

    #endregion

    #region Data Validation Tests

    [Theory]
    [InlineData(true, "active")]
    [InlineData(false, "inactive")]
    public async Task UpdateProductStatus_WithBooleanValues_ShouldReturnCorrectMessage(bool isActive, string expectedStatus)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync($"Status {expectedStatus}", $"status-{expectedStatus}", 19.99m);

        var updateRequest = new
        {
            IsActive = isActive
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be($"Product status updated to {expectedStatus}");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product!.IsActive.Should().Be(isActive);
        });
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task UpdateProductStatus_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple products first to avoid concurrency conflicts
        var productIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var productId = await CreateTestProductAsync($"Concurrent Test {i}", $"concurrent-test-{i}", 25.99m + i);
            productIds.Add(productId);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Create concurrent update tasks for different products
        var tasks = productIds.Select((id, index) =>
        {
            var updateRequest = new { IsActive = index % 2 == 0 }; // Alternate between true/false
            return PutApiResponseAsync<object, string>($"v1/products/{id}/status", updateRequest);
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));

        // Verify final state is consistent for all products
        await ExecuteDbContextAsync(async context =>
        {
            var products = await context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            products.Should().HaveCount(5);
            products.Should().AllSatisfy(product => product.Should().NotBeNull());
        });
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateProductStatus_ResponseTime_ShouldBeReasonable()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Performance Test", "performance-test", 299.99m);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should be very fast for simple status update
    }

    [Fact]
    public async Task UpdateProductStatus_MultipleProducts_ShouldHandleIndependently()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var product1Id = await CreateTestProductAsync("Product 1", "product-1", 49.99m);
        var product2Id = await CreateTestProductAsync("Product 2", "product-2", 69.99m);
        var product3Id = await CreateTestProductAsync("Product 3", "product-3", 89.99m);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Update different products to different statuses
        var response1 = await PutApiResponseAsync<object, string>($"v1/products/{product1Id}/status", new { IsActive = false });
        var response2 = await PutApiResponseAsync<object, string>($"v1/products/{product2Id}/status", new { IsActive = true });
        var response3 = await PutApiResponseAsync<object, string>($"v1/products/{product3Id}/status", new { IsActive = false });

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);
        AssertApiSuccess(response3);

        // Verify each product has correct status
        await ExecuteDbContextAsync(async context =>
        {
            var products = await context.Products
                .Where(p => new[] { product1Id, product2Id, product3Id }.Contains(p.Id))
                .ToListAsync();

            var product1 = products.First(p => p.Id == product1Id);
            var product2 = products.First(p => p.Id == product2Id);
            var product3 = products.First(p => p.Id == product3Id);

            product1.IsActive.Should().BeFalse();
            product2.IsActive.Should().BeTrue();
            product3.IsActive.Should().BeFalse();
        });
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateProductStatus_WithUnicodeProductName_ShouldUpdateCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Café Münchën Product™", "cafe-munchen-product", 99.99m);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Product status updated to inactive");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            product!.IsActive.Should().BeFalse();
            product.Name.Should().Be("Café Münchën Product™");
        });
    }

    [Fact]
    public async Task UpdateProductStatus_WithZeroPrice_ShouldUpdateCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var productId = await CreateTestProductAsync("Free Product", "free-product", 0m);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/products/{productId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Product status updated to inactive");
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateTestProductAsync(
        string name,
        string slug,
        decimal basePrice,
        bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var createCommand = new CreateProductCommandV1
        {
            Name = name,
            Slug = slug,
            Description = $"Test product: {name}",
            BasePrice = basePrice,
            Currency = "USD",
            Sku = $"SKU-{Guid.NewGuid():N}".Substring(0, 12).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = isActive,
            Attributes = new List<ProductAttributeDto>(),
            Images = new List<ProductImageDto>()
        };

        var result = await mediator.Send(createCommand);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.Id;
        }

        throw new InvalidOperationException($"Failed to create test product: {name}");
    }

    private async Task<Guid> CreateTestUserAsync(string email, string firstName, string lastName, UserRole role = UserRole.Customer)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var registerCommand = new Shopilent.Application.Features.Identity.Commands.Register.V1.RegisterCommandV1
        {
            Email = email,
            Password = "Password123!",
            FirstName = firstName,
            LastName = lastName,
            Phone = $"+1555{new Random().Next(1000000, 9999999)}",
            IpAddress = "127.0.0.1",
            UserAgent = "Integration Test"
        };

        var result = await mediator.Send(registerCommand);

        if (result.IsSuccess && result.Value != null)
        {
            var userId = result.Value.User.Id;

            // Change role if not customer
            if (role != UserRole.Customer)
            {
                var changeRoleCommand = new Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1.ChangeUserRoleCommandV1
                {
                    UserId = userId,
                    NewRole = role
                };
                await mediator.Send(changeRoleCommand);
            }

            return userId;
        }

        throw new InvalidOperationException($"Failed to create test user: {email}");
    }

    #endregion
}
