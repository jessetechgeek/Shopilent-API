using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Features.Administration.Commands.ClearAllCache.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Administration.ClearAllCache.V1;

public class ClearAllCacheEndpointV1Tests : ApiIntegrationTestBase
{
    public ClearAllCacheEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ClearAllCache_WithValidAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClearAllCacheResponseV1>>(
            content, JsonOptions);

        AssertApiSuccess(apiResponse);
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data.Message.Should().NotBeNullOrEmpty();
        apiResponse.Data.ClearedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        apiResponse.Data.KeysCleared.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ClearAllCache_WithCacheEntries_ShouldClearAllEntries()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Pre-populate cache with test data using string values for better compatibility
        var cacheService = GetCacheService();
        var testCacheKeys = new[]
        {
            "test:product:123",
            "test:category:abc", 
            "test:user:profile:1",
            "test:session:token123"
        };
        
        // Set cache entries with string values to avoid serialization issues
        foreach (var key in testCacheKeys)
        {
            await cacheService.SetAsync(key, $"test-value-{key}", TimeSpan.FromHours(1));
        }

        // Verify cache entries exist before clearing
        var keysExistBeforeClear = 0;
        foreach (var key in testCacheKeys)
        {
            var exists = await cacheService.ExistsAsync(key);
            if (exists)
            {
                keysExistBeforeClear++;
            }
        }
        
        keysExistBeforeClear.Should().BeGreaterThan(0, "should have cache entries before clearing");

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClearAllCacheResponseV1>>(
            content, JsonOptions);

        AssertApiSuccess(apiResponse);
        apiResponse!.Data.KeysCleared.Should().BeGreaterThan(0);

        // Verify cache entries are cleared using ExistsAsync for better reliability
        foreach (var key in testCacheKeys)
        {
            var exists = await cacheService.ExistsAsync(key);
            exists.Should().BeFalse($"cache key {key} should be cleared");
        }
    }

    [Fact]
    public async Task ClearAllCache_WithEmptyCache_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Ensure cache is empty by clearing first
        var cacheService = GetCacheService();
        await cacheService.ClearAllAsync();

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClearAllCacheResponseV1>>(
            content, JsonOptions);

        AssertApiSuccess(apiResponse);
        apiResponse!.Data.KeysCleared.Should().BeGreaterOrEqualTo(0);
        apiResponse.Data.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ClearAllCache_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ClearAllCache_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ClearAllCache_WithDifferentCachePatterns_ShouldClearAllPatterns()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var cacheService = GetCacheService();
        
        // Create entries with different prefixes using simple string values
        var cacheEntries = new Dictionary<string, string>
        {
            ["product:test-123"] = "Product Data 1",
            ["product:test-456"] = "Product Data 2", 
            ["category:test-abc"] = "Category Electronics",
            ["category:test-def"] = "Category Clothing",
            ["user:profile:test-1"] = "User Profile Data",
            ["user:session:test-token"] = "Session Data"
        };

        // Set all cache entries
        foreach (var entry in cacheEntries)
        {
            await cacheService.SetAsync(entry.Key, entry.Value, TimeSpan.FromHours(1));
        }

        // Verify entries exist before clearing
        var entriesExistBeforeClear = 0;
        foreach (var entry in cacheEntries)
        {
            if (await cacheService.ExistsAsync(entry.Key))
                entriesExistBeforeClear++;
        }
        
        entriesExistBeforeClear.Should().BeGreaterThan(0, "should have cache entries before clearing");

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClearAllCacheResponseV1>>(
            content, JsonOptions);

        AssertApiSuccess(apiResponse);

        // Verify all patterns are cleared using ExistsAsync
        foreach (var entry in cacheEntries)
        {
            var exists = await cacheService.ExistsAsync(entry.Key);
            exists.Should().BeFalse($"cache key {entry.Key} should be cleared");
        }
    }

    [Fact]
    public async Task ClearAllCache_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Pre-populate cache with simple test data
        var cacheService = GetCacheService();
        var testKeys = new[]
        {
            "concurrent:test:key1",
            "concurrent:test:key2", 
            "concurrent:test:key3",
            "concurrent:test:key4",
            "concurrent:test:key5"
        };
        
        foreach (var key in testKeys)
        {
            await cacheService.SetAsync(key, $"concurrent-test-value-{key}", TimeSpan.FromHours(1));
        }

        // Verify we have cache entries before concurrent clearing
        var keysExistBeforeClear = 0;
        foreach (var key in testKeys)
        {
            if (await cacheService.ExistsAsync(key))
                keysExistBeforeClear++;
        }
        
        keysExistBeforeClear.Should().BeGreaterThan(0, "should have cache entries before clearing");

        // Act - Send multiple concurrent clear requests
        var tasks = Enumerable.Range(0, 3)
            .Select(_ => DeleteAsync("v1/administration/cache"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.OK));

        // At least one request should report clearing some keys
        var totalKeysCleared = 0;
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClearAllCacheResponseV1>>(
                content, JsonOptions);
            
            AssertApiSuccess(apiResponse);
            totalKeysCleared += apiResponse!.Data.KeysCleared;
        }

        totalKeysCleared.Should().BeGreaterThan(0, "at least one request should report clearing cache keys");
    }

    [Fact]
    public async Task ClearAllCache_AfterCachingQueries_ShouldClearQueryCache()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var cacheService = GetCacheService();
        
        // Simulate cached query results (like GetAllCategories, GetAllProducts, etc.)
        var queryCacheKeys = new[]
        {
            $"query:GetAllCategoriesQueryV1:{Guid.NewGuid()}",
            $"query:GetAllProductsQueryV1:{Guid.NewGuid()}",
            $"query:GetUserProfileQueryV1:{Guid.NewGuid()}"
        };

        // Set query cache entries with simple string values
        foreach (var key in queryCacheKeys)
        {
            await cacheService.SetAsync(key, $"cached-query-result-{key}", TimeSpan.FromHours(1));
        }

        // Verify query cache entries exist before clearing
        var queryCacheEntriesExist = 0;
        foreach (var key in queryCacheKeys)
        {
            if (await cacheService.ExistsAsync(key))
                queryCacheEntriesExist++;
        }
        
        queryCacheEntriesExist.Should().BeGreaterThan(0, "should have query cache entries before clearing");

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClearAllCacheResponseV1>>(
            content, JsonOptions);

        AssertApiSuccess(apiResponse);
        apiResponse!.Data.KeysCleared.Should().BeGreaterThan(0);

        // Verify query cache entries are cleared
        foreach (var key in queryCacheKeys)
        {
            var exists = await cacheService.ExistsAsync(key);
            exists.Should().BeFalse($"query cache key {key} should be cleared");
        }
    }

    [Fact]
    public async Task ClearAllCache_ShouldReturnCorrectResponseFormat()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClearAllCacheResponseV1>>(
            content, JsonOptions);

        // Verify API response structure
        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeTrue();
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(200);
        apiResponse.Data.Should().NotBeNull();

        // Verify clear cache response structure
        apiResponse.Data.Message.Should().NotBeNullOrEmpty();
        apiResponse.Data.ClearedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        apiResponse.Data.KeysCleared.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ClearAllCache_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange - Use an obviously invalid token
        SetAuthenticationHeader("invalid.expired.token");

        // Act
        var response = await DeleteAsync("v1/administration/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private ICacheService GetCacheService()
    {
        using var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ICacheService>();
    }

}