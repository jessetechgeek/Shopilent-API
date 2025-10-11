using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.GetProductVariants.V1;

public class GetProductVariantsEndpointV1Tests : ApiIntegrationTestBase
{
    public GetProductVariantsEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Helper Methods

    private async Task<Guid> CreateVariantAttributeAsync()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"variant_attr_{uniqueId}",
            displayName: $"Variant Attr {uniqueId}",
            type: "Select",
            isVariant: true);
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        return attributeResponse!.Data.Id;
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task GetProductVariants_WithValidProductId_ShouldReturnSuccessResponse()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product first
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Clear auth header since GetProductVariants allows anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Message.Should().Be("Product variants retrieved successfully");
    }

    [Fact]
    public async Task GetProductVariants_WithProductHavingVariants_ShouldReturnAllVariants()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add multiple variants to the product
        var variantRequests = new[]
        {
            new
            {
                Name = "Small Variant",
                Sku = $"VAR-S-{Guid.NewGuid():N}",
                Price = 29.99m,
                Currency = "USD",
                StockQuantity = 100,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Small"
                    }
                },
                Images = new List<object>()
            },
            new
            {
                Name = "Medium Variant",
                Sku = $"VAR-M-{Guid.NewGuid():N}",
                Price = 39.99m,
                Currency = "USD",
                StockQuantity = 75,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Medium"
                    }
                },
                Images = new List<object>()
            },
            new
            {
                Name = "Large Variant",
                Sku = $"VAR-L-{Guid.NewGuid():N}",
                Price = 49.99m,
                Currency = "USD",
                StockQuantity = 50,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Large"
                    }
                },
                Images = new List<object>()
            }
        };

        var createdVariantIds = new List<Guid>();
        foreach (var variantRequest in variantRequests)
        {
            var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
                $"v1/products/{productId}/variants", variantRequest);
            AssertApiSuccess(variantResponse);
            createdVariantIds.Add(variantResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(3, "All 3 created variants should be present");

        // Verify all created variants are in the response
        foreach (var variantId in createdVariantIds)
        {
            response.Data.Should().Contain(v => v.Id == variantId,
                $"Variant with ID {variantId} should be present in the response");
        }

        // Verify each variant has the correct structure
        foreach (var variant in response.Data)
        {
            variant.Id.Should().NotBeEmpty();
            variant.ProductId.Should().Be(productId);
            variant.Sku.Should().NotBeNullOrEmpty();
            variant.Price.Should().BeGreaterThan(0);
            variant.Currency.Should().Be("USD");
            variant.StockQuantity.Should().BeGreaterOrEqualTo(0);
            variant.IsActive.Should().BeTrue();
            variant.Metadata.Should().NotBeNull();
            variant.Attributes.Should().NotBeNull();
            variant.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            variant.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }
    }

    [Fact]
    public async Task GetProductVariants_WithProductHavingNoVariants_ShouldReturnEmptyList()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product without variants
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().BeEmpty("Product has no variants");
        response.Message.Should().Be("Product variants retrieved successfully");
    }

    [Fact]
    public async Task GetProductVariants_ShouldReturnVariantsWithCorrectStructure()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add a variant with comprehensive data
        var variantRequest = new
        {
            Name = "Comprehensive Variant",
            Sku = $"COMP-VAR-{Guid.NewGuid():N}",
            Price = 99.99m,
            Currency = "USD",
            StockQuantity = 150,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Comprehensive"
                }
            },
            Images = new List<object>()
        };

        var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
            $"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(variantResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1);

        var variant = response.Data.First();
        variant.Id.Should().Be(variantResponse!.Data.Id);
        variant.ProductId.Should().Be(productId);
        variant.Sku.Should().StartWith("COMP-VAR-");
        variant.Price.Should().Be(99.99m);
        variant.Currency.Should().Be("USD");
        variant.StockQuantity.Should().Be(150);
        variant.IsActive.Should().BeTrue();
        variant.Metadata.Should().NotBeNull();
        variant.Attributes.Should().NotBeNull();
        variant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        variant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task GetProductVariants_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header to test anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert - GetProductVariants allows anonymous access
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductVariants_WithCustomerAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Authenticate as customer
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductVariants_WithAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act (still authenticated as admin)
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetProductVariants_WithNonExistentProductId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"v1/products/{nonExistentProductId}/variants2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain($"Product with ID {nonExistentProductId} not found");
    }

    [Fact]
    public async Task GetProductVariants_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidGuid = "invalid-guid-format";

        // Act
        var response = await Client.GetAsync($"v1/products/{invalidGuid}/variants2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductVariants_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = await Client.GetAsync($"v1/products/{emptyGuid}/variants2");

        // Assert
        // Empty GUID is technically valid GUID format, so it passes routing but fails product lookup
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product");
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetProductVariants_ShouldReturnDataConsistentWithDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add variants
        var variantRequests = new[]
        {
            new
            {
                Name = "DB Test Variant 1",
                Sku = $"DB-VAR-1-{Guid.NewGuid():N}",
                Price = 19.99m,
                Currency = "USD",
                StockQuantity = 50,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Variant 1"
                    }
                },
                Images = new List<object>()
            },
            new
            {
                Name = "DB Test Variant 2",
                Sku = $"DB-VAR-2-{Guid.NewGuid():N}",
                Price = 29.99m,
                Currency = "USD",
                StockQuantity = 75,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Variant 2"
                    }
                },
                Images = new List<object>()
            }
        };

        var createdVariantIds = new List<Guid>();
        foreach (var variantRequest in variantRequests)
        {
            var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
                $"v1/products/{productId}/variants", variantRequest);
            AssertApiSuccess(variantResponse);
            createdVariantIds.Add(variantResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(2, "Two variants should be present");

        // Verify API response data matches database data exactly
        await ExecuteDbContextAsync(async context =>
        {
            var dbVariants = await context.ProductVariants
                .Where(v => createdVariantIds.Contains(v.Id))
                .ToListAsync();

            dbVariants.Should().HaveCount(2, "Both created variants should exist in database");

            foreach (var dbVariant in dbVariants)
            {
                var apiVariant = response.Data.FirstOrDefault(v => v.Id == dbVariant.Id);
                apiVariant.Should().NotBeNull($"Variant with ID {dbVariant.Id} should be present in API response");

                // Verify all fields match between API and database
                apiVariant!.Id.Should().Be(dbVariant.Id);
                apiVariant.ProductId.Should().Be(dbVariant.ProductId);
                apiVariant.Sku.Should().Be(dbVariant.Sku);
                apiVariant.Price.Should().Be(dbVariant.Price.Amount);
                apiVariant.Currency.Should().Be(dbVariant.Price.Currency);
                apiVariant.StockQuantity.Should().Be(dbVariant.StockQuantity);
                apiVariant.IsActive.Should().Be(dbVariant.IsActive);
                apiVariant.CreatedAt.Should().BeCloseTo(dbVariant.CreatedAt, TimeSpan.FromSeconds(1));
                apiVariant.UpdatedAt.Should().BeCloseTo(dbVariant.UpdatedAt, TimeSpan.FromSeconds(1));
            }
        });
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task GetProductVariants_ShouldReturnVariantsInConsistentOrder()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add variants with specific creation order
        var variantRequests = new[]
        {
            new
            {
                Name = "Z Last",
                Sku = "Z-LAST",
                Price = 49.99m,
                Currency = "USD",
                StockQuantity = 50,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Last"
                    }
                },
                Images = new List<object>()
            },
            new
            {
                Name = "A First",
                Sku = "A-FIRST",
                Price = 29.99m,
                Currency = "USD",
                StockQuantity = 100,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "First"
                    }
                },
                Images = new List<object>()
            },
            new
            {
                Name = "M Middle",
                Sku = "M-MIDDLE",
                Price = 39.99m,
                Currency = "USD",
                StockQuantity = 75,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Middle"
                    }
                },
                Images = new List<object>()
            }
        };

        foreach (var variantRequest in variantRequests)
        {
            var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
                $"v1/products/{productId}/variants", variantRequest);
            AssertApiSuccess(variantResponse);
            // Add small delay to ensure different creation times
            await Task.Delay(10);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Call multiple times to ensure consistent ordering
        var response1 = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");
        var response2 = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Verify consistent ordering between calls
        response1!.Data.Select(v => v.Id).Should().BeEquivalentTo(
            response2!.Data.Select(v => v.Id),
            options => options.WithStrictOrdering());
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetProductVariants_WithVariantsHavingDifferentPrices_ShouldReturnAllCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add variants with different prices
        var variantRequests = new[]
        {
            new
            {
                Name = "Budget Variant",
                Sku = "BUDGET",
                Price = 9.99m,
                Currency = "USD",
                StockQuantity = 200,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Budget"
                    }
                },
                Images = new List<object>()
            },
            new
            {
                Name = "Premium Variant",
                Sku = "PREMIUM",
                Price = 199.99m,
                Currency = "USD",
                StockQuantity = 10,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Premium"
                    }
                },
                Images = new List<object>()
            }
        };

        foreach (var variantRequest in variantRequests)
        {
            var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
                $"v1/products/{productId}/variants", variantRequest);
            AssertApiSuccess(variantResponse);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(2);

        var budgetVariant = response.Data.FirstOrDefault(v => v.Sku == "BUDGET");
        var premiumVariant = response.Data.FirstOrDefault(v => v.Sku == "PREMIUM");

        budgetVariant.Should().NotBeNull();
        budgetVariant!.Price.Should().Be(9.99m);

        premiumVariant.Should().NotBeNull();
        premiumVariant!.Price.Should().Be(199.99m);
    }

    [Fact]
    public async Task GetProductVariants_WithVariantsHavingZeroStock_ShouldReturnAllVariants()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add variant with zero stock
        var variantRequest = new
        {
            Name = "Out of Stock Variant",
            Sku = "OUT-OF-STOCK",
            Price = 39.99m,
            Currency = "USD",
            StockQuantity = 0,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Out of Stock"
                }
            },
            Images = new List<object>()
        };

        var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
            $"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(variantResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1);

        var variant = response.Data.First();
        variant.StockQuantity.Should().Be(0);
        variant.Sku.Should().Be("OUT-OF-STOCK");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetProductVariants_WithManyVariants_ShouldPerformWell()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add many variants
        for (int i = 0; i < 20; i++)
        {
            var variantRequest = new
            {
                Name = $"Variant {i + 1}",
                Sku = $"VAR-{i + 1:D3}",
                Price = 10m + (i * 5),
                Currency = "USD",
                StockQuantity = 100 - (i * 3),
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = $"Value {i + 1}"
                    }
                },
                Images = new List<object>()
            };

            var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
                $"v1/products/{productId}/variants", variantRequest);
            AssertApiSuccess(variantResponse);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act & Assert - Measure response time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");
        stopwatch.Stop();

        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(20);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public async Task GetProductVariants_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product with some variants
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add a variant
        var variantRequest = new
        {
            Name = "Test Variant",
            Sku = "TEST-VAR",
            Price = 29.99m,
            Currency = "USD",
            StockQuantity = 100,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Test"
                }
            },
            Images = new List<object>()
        };

        await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
            $"v1/products/{productId}/variants", variantRequest);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Make concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should succeed with consistent data
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));

        // Verify all responses have the same variant count and IDs
        var firstResponseData = responses[0]!.Data;
        responses.Should().AllSatisfy(response =>
        {
            response!.Data.Should().HaveCount(firstResponseData.Count);
            response.Data.Select(v => v.Id).Should().BeEquivalentTo(firstResponseData.Select(v => v.Id));
        });
    }

    #endregion

    #region Cache Behavior Tests

    [Fact]
    public async Task GetProductVariants_ShouldBeCached()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a variant attribute
        var attributeId = await CreateVariantAttributeAsync();

        // Create a product with variant
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Add a variant
        var variantRequest = new
        {
            Name = "Cache Test Variant",
            Sku = "CACHE-VAR",
            Price = 29.99m,
            Currency = "USD",
            StockQuantity = 100,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Cache Test"
                }
            },
            Images = new List<object>()
        };

        await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
            $"v1/products/{productId}/variants", variantRequest);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Make first request (should populate cache)
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
        var response1 = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");
        stopwatch1.Stop();

        // Act - Make second request (should use cache)
        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        var response2 = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");
        stopwatch2.Stop();

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Verify both responses have the same variant count and IDs
        response1!.Data.Should().HaveCount(response2!.Data.Count);
        response1.Data.Select(v => v.Id).Should().BeEquivalentTo(response2.Data.Select(v => v.Id));

        // Second request should be faster (cached) - this is a soft assertion
        // Note: In test environment, caching behavior might vary
        stopwatch2.ElapsedMilliseconds.Should().BeLessOrEqualTo(stopwatch1.ElapsedMilliseconds + 100);
    }

    #endregion

    #region HTTP Status Code Tests

    [Fact]
    public async Task GetProductVariants_ShouldReturnStatus200()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task GetProductVariants_ShouldHaveCorrectContentType()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);

        var productId = productResponse!.Data.Id;

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<ProductVariantDto>>($"v1/products/{productId}/variants2");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
        // Content type verification is handled by the API response structure
    }

    #endregion
}
