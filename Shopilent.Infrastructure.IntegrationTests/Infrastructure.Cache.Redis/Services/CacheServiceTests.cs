using Shopilent.Application.Abstractions.Caching;
using Shopilent.Infrastructure.IntegrationTests.Common;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Cache.Redis.Services;

[Collection("IntegrationTests")]
public class CacheServiceTests : IntegrationTestBase
{
    private ICacheService _cacheService = null!;

    public CacheServiceTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _cacheService = GetService<ICacheService>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SetAsync_ValidKeyValue_ShouldPersistToRedis()
    {
        // Arrange
        await ClearCacheAsync();
        const string key = "test:cache:key";
        const string value = "test cache value";
        var expiry = TimeSpan.FromMinutes(5);

        // Act
        await _cacheService.SetAsync(key, value, expiry);

        // Assert
        var cachedValue = await _cacheService.GetAsync<string>(key);
        cachedValue.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_ExistingKey_ShouldReturnCachedValue()
    {
        // Arrange
        await ClearCacheAsync();
        const string key = "test:get:key";
        const string expectedValue = "cached value";

        await _cacheService.SetAsync(key, expectedValue, TimeSpan.FromMinutes(5));

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ShouldReturnDefault()
    {
        // Arrange
        await ClearCacheAsync();
        const string nonExistentKey = "non:existent:key";

        // Act
        var result = await _cacheService.GetAsync<string>(nonExistentKey);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ExistingKey_ShouldDeleteFromCache()
    {
        // Arrange
        await ClearCacheAsync();
        const string key = "test:remove:key";
        const string value = "value to remove";

        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        var cachedValue = await _cacheService.GetAsync<string>(key);
        cachedValue.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_NewKey_ShouldExecuteFactoryAndCache()
    {
        // Arrange
        await ClearCacheAsync();
        const string key = "test:get:or:create:key";
        const string expectedValue = "factory generated value";
        var factoryExecuted = false;

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, () =>
        {
            factoryExecuted = true;
            return Task.FromResult(expectedValue);
        }, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be(expectedValue);
        factoryExecuted.Should().BeTrue();

        // Verify it was cached
        var cachedValue = await _cacheService.GetAsync<string>(key);
        cachedValue.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_ExistingKey_ShouldReturnCachedValueWithoutExecutingFactory()
    {
        // Arrange
        await ClearCacheAsync();
        const string key = "test:existing:key";
        const string cachedValue = "already cached";
        const string factoryValue = "should not be called";
        var factoryExecuted = false;

        await _cacheService.SetAsync(key, cachedValue, TimeSpan.FromMinutes(5));

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, () =>
        {
            factoryExecuted = true;
            return Task.FromResult(factoryValue);
        }, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be(cachedValue);
        factoryExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveByPatternAsync_MatchingPattern_ShouldRemoveMatchingKeys()
    {
        // Arrange
        await ClearCacheAsync();

        await _cacheService.SetAsync("product:1", "Product 1", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("product:2", "Product 2", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("category:1", "Category 1", TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.RemoveByPatternAsync("product:*");

        // Assert
        var product1 = await _cacheService.GetAsync<string>("product:1");
        var product2 = await _cacheService.GetAsync<string>("product:2");
        var category1 = await _cacheService.GetAsync<string>("category:1");

        product1.Should().BeNull();
        product2.Should().BeNull();
        category1.Should().Be("Category 1"); // Should not be removed
    }

    [Fact]
    public async Task SetAsync_ComplexObject_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        await ClearCacheAsync();
        const string key = "test:complex:object";

        var complexObject = new TestComplexObject
        {
            Id = Guid.NewGuid(),
            Name = "Test Object",
            CreatedAt = DateTime.UtcNow,
            Properties = new Dictionary<string, object>
            {
                { "StringProp", "String Value" },
                { "IntProp", 42 },
                { "BoolProp", true }
            }
        };

        // Act
        await _cacheService.SetAsync(key, complexObject, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestComplexObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(complexObject.Id);
        result.Name.Should().Be(complexObject.Name);
        result.CreatedAt.Should().Be(complexObject.CreatedAt);
        result.Properties.Should().HaveCount(3);
        result.Properties["StringProp"].ToString().Should().Be("String Value");
    }

    [Fact]
    public async Task SetAsync_WithShortExpiry_ShouldExpireAutomatically()
    {
        // Arrange
        await ClearCacheAsync();
        const string key = "test:expiry:key";
        const string value = "expires quickly";

        // Act
        await _cacheService.SetAsync(key, value, TimeSpan.FromMilliseconds(100));

        // Wait for expiration
        await Task.Delay(TimeSpan.FromMilliseconds(150));

        // Assert
        var cachedValue = await _cacheService.GetAsync<string>(key);
        cachedValue.Should().BeNull();
    }

    private class TestComplexObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
