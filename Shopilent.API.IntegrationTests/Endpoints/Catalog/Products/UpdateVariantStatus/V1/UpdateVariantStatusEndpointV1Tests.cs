using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;
using Shopilent.Domain.Identity.Enums;
using CreateProductCommand = Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;
using AddVariantCommand = Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.UpdateVariantStatus.V1;

public class UpdateVariantStatusEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateVariantStatusEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateVariantStatus_ActivateVariant_ShouldSetToActive()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Status Test Product", "status-test-product", "VAR-STATUS-001", isActive: false);

        var updateRequest = new
        {
            IsActive = true
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Variant status updated to active");
        response.Message.Should().Be("Variant status updated successfully");
    }

    [Fact]
    public async Task UpdateVariantStatus_DeactivateVariant_ShouldSetToInactive()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Deactivate Test", "deactivate-test", "VAR-DEACT-001", isActive: true);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Variant status updated to inactive");
        response.Message.Should().Be("Variant status updated successfully");
    }

    [Fact]
    public async Task UpdateVariantStatus_UpdateInDatabase_ShouldPersistChanges()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Database Test", "database-test", "VAR-DB-001");

        // First deactivate
        var deactivateRequest = new { IsActive = false };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate
        var deactivateResponse = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", deactivateRequest);

        // Assert deactivation
        AssertApiSuccess(deactivateResponse);

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            variant.Should().NotBeNull();
            variant!.IsActive.Should().BeFalse();
        });

        // Act - Reactivate
        var activateRequest = new { IsActive = true };
        var activateResponse = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", activateRequest);

        // Assert reactivation
        AssertApiSuccess(activateResponse);

        // Verify in database again
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            variant.Should().NotBeNull();
            variant!.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task UpdateVariantStatus_WithManagerRole_ShouldAllowStatusUpdate()
    {
        // Arrange
        await CreateTestUserAsync("manager@test.com", "Manager", "User", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@test.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Manager Test", "manager-test", "VAR-MGR-001");

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Variant status updated to inactive");
    }

    [Fact]
    public async Task UpdateVariantStatus_ToggleMultipleTimes_ShouldHandleCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Toggle Test", "toggle-test", "VAR-TOG-001");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate
        var deactivateRequest = new { IsActive = false };
        var deactivateResponse = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", deactivateRequest);

        // Act - Reactivate
        var activateRequest = new { IsActive = true };
        var activateResponse = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", activateRequest);

        // Act - Deactivate again
        var deactivateAgainResponse = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", deactivateRequest);

        // Assert
        AssertApiSuccess(deactivateResponse);
        AssertApiSuccess(activateResponse);
        AssertApiSuccess(deactivateAgainResponse);

        deactivateResponse!.Data.Should().Be("Variant status updated to inactive");
        activateResponse!.Data.Should().Be("Variant status updated to active");
        deactivateAgainResponse!.Data.Should().Be("Variant status updated to inactive");

        // Verify final state in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            variant!.IsActive.Should().BeFalse();
        });
    }

    [Fact]
    public async Task UpdateVariantStatus_SameStatus_ShouldStillReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Same Status Test", "same-status-test", "VAR-SAME-001", isActive: true);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Variant starts as active, try to activate again
        var updateRequest = new { IsActive = true };

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Variant status updated to active");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            variant!.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task UpdateVariantStatus_MultipleVariantsOfProduct_ShouldUpdateIndependently()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product with multiple variants
        var productId = await CreateTestProductAsync("Multi Variant Product", "multi-variant-product", 99.99m);
        var variant1Id = await CreateTestVariantAsync(productId, "VAR-MULTI-001", 89.99m);
        var variant2Id = await CreateTestVariantAsync(productId, "VAR-MULTI-002", 99.99m);
        var variant3Id = await CreateTestVariantAsync(productId, "VAR-MULTI-003", 109.99m);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Update different variants to different statuses
        var response1 = await PutApiResponseAsync<object, string>($"v1/variants/{variant1Id}/status", new { IsActive = false });
        var response2 = await PutApiResponseAsync<object, string>($"v1/variants/{variant2Id}/status", new { IsActive = true });
        var response3 = await PutApiResponseAsync<object, string>($"v1/variants/{variant3Id}/status", new { IsActive = false });

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);
        AssertApiSuccess(response3);

        // Verify each variant has correct status
        await ExecuteDbContextAsync(async context =>
        {
            var variants = await context.ProductVariants
                .Where(v => new[] { variant1Id, variant2Id, variant3Id }.Contains(v.Id))
                .ToListAsync();

            var variant1 = variants.First(v => v.Id == variant1Id);
            var variant2 = variants.First(v => v.Id == variant2Id);
            var variant3 = variants.First(v => v.Id == variant3Id);

            variant1.IsActive.Should().BeFalse();
            variant2.IsActive.Should().BeTrue();
            variant3.IsActive.Should().BeFalse();
        });
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task UpdateVariantStatus_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        var variantId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVariantStatus_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateVariantStatus_WithNonExistentVariant_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentVariantId = Guid.NewGuid();
        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/variants/{nonExistentVariantId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateVariantStatus_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync("v1/variants/invalid-guid/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateVariantStatus_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            IsActive = false
        };

        // Act
        var response = await PutAsync($"v1/variants/{Guid.Empty}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task UpdateVariantStatus_DeactivateVariant_ShouldNotAffectProduct()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Product Independent Test", "product-independent-test", "VAR-IND-001");

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Deactivate variant
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify variant is deactivated but product remains active
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            variant!.IsActive.Should().BeFalse();
            product!.IsActive.Should().BeTrue(); // Product should remain active
        });
    }

    [Fact]
    public async Task UpdateVariantStatus_WithStockQuantity_ShouldPreserveStock()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Stock Test", "stock-test", "VAR-STOCK-001", stockQuantity: 50);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify stock quantity is preserved
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            variant!.IsActive.Should().BeFalse();
            variant.StockQuantity.Should().Be(50); // Stock should remain unchanged
        });
    }

    #endregion

    #region Data Validation Tests

    [Theory]
    [InlineData(true, "active")]
    [InlineData(false, "inactive")]
    public async Task UpdateVariantStatus_WithBooleanValues_ShouldReturnCorrectMessage(bool isActive, string expectedStatus)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync($"Status {expectedStatus}", $"status-{expectedStatus}", $"VAR-{expectedStatus.ToUpper()}-001");

        var updateRequest = new
        {
            IsActive = isActive
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be($"Variant status updated to {expectedStatus}");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            variant!.IsActive.Should().Be(isActive);
        });
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task UpdateVariantStatus_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple variants first to avoid concurrency conflicts
        var variantIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var (productId, variantId) = await CreateProductAndVariantAsync($"Concurrent Test {i}", $"concurrent-test-{i}", $"VAR-CONC-{i:D3}");
            variantIds.Add(variantId);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Create concurrent update tasks for different variants
        var tasks = variantIds.Select((id, index) =>
        {
            var updateRequest = new { IsActive = index % 2 == 0 }; // Alternate between true/false
            return PutApiResponseAsync<object, string>($"v1/variants/{id}/status", updateRequest);
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));

        // Verify final state is consistent for all variants
        await ExecuteDbContextAsync(async context =>
        {
            var variants = await context.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync();

            variants.Should().HaveCount(5);
            variants.Should().AllSatisfy(variant => variant.Should().NotBeNull());
        });
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateVariantStatus_ResponseTime_ShouldBeReasonable()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Performance Test", "performance-test", "VAR-PERF-001");

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should be very fast for simple status update
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateVariantStatus_WithZeroPrice_ShouldUpdateCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Free Variant", "free-variant", "VAR-FREE-001", price: 0m);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Variant status updated to inactive");
    }

    [Fact]
    public async Task UpdateVariantStatus_WithZeroStock_ShouldUpdateCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId) = await CreateProductAndVariantAsync("Out Of Stock", "out-of-stock", "VAR-OOS-001", stockQuantity: 0);

        var updateRequest = new
        {
            IsActive = false
        };

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/variants/{variantId}/status", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Variant status updated to inactive");

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
            variant!.IsActive.Should().BeFalse();
            variant.StockQuantity.Should().Be(0);
        });
    }

    #endregion

    #region Helper Methods

    private async Task<(Guid ProductId, Guid VariantId)> CreateProductAndVariantAsync(
        string productName,
        string productSlug,
        string variantSku,
        decimal price = 99.99m,
        int stockQuantity = 10,
        bool isActive = true)
    {
        var productId = await CreateTestProductAsync(productName, productSlug, 100m);
        var variantId = await CreateTestVariantAsync(productId, variantSku, price, stockQuantity, isActive);
        return (productId, variantId);
    }

    private async Task<Guid> CreateTestProductAsync(
        string name,
        string slug,
        decimal basePrice,
        bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var createCommand = new CreateProductCommand.CreateProductCommandV1
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
            Attributes = new List<CreateProductCommand.ProductAttributeDto>(),
            Images = new List<CreateProductCommand.ProductImageDto>()
        };

        var result = await mediator.Send(createCommand);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.Id;
        }

        throw new InvalidOperationException($"Failed to create test product: {name}");
    }

    private async Task<Guid> CreateTestVariantAsync(
        Guid productId,
        string sku,
        decimal price = 99.99m,
        int stockQuantity = 10,
        bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var addVariantCommand = new AddProductVariantCommandV1
        {
            ProductId = productId,
            Sku = sku,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            Attributes = new List<AddVariantCommand.ProductAttributeDto>(),
            Metadata = new Dictionary<string, object>(),
            Images = new List<AddVariantCommand.ProductImageDto>()
        };

        var result = await mediator.Send(addVariantCommand);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.Id;
        }

        throw new InvalidOperationException($"Failed to create test variant: {sku}");
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
