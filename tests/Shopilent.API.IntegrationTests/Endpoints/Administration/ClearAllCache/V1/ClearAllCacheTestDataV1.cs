using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Administration.ClearAllCache.V1;

public static class ClearAllCacheTestDataV1
{
    private static readonly Faker _faker = new();

    public static class CacheData
    {
        public static string GenerateRandomCacheKey(string? prefix = null)
        {
            prefix ??= _faker.Random.Word();
            return $"{prefix}:{_faker.Random.AlphaNumeric(10)}";
        }

        public static object GenerateRandomCacheValue()
        {
            return new
            {
                Id = _faker.Random.Guid(),
                Name = _faker.Commerce.ProductName(),
                Value = _faker.Random.Decimal(1, 1000),
                CreatedAt = _faker.Date.Recent()
            };
        }

        public static Dictionary<string, object> GenerateMultipleCacheEntries(int count = 5)
        {
            var entries = new Dictionary<string, object>();
            for (int i = 0; i < count; i++)
            {
                var key = GenerateRandomCacheKey();
                var value = GenerateRandomCacheValue();
                entries[key] = value;
            }
            return entries;
        }
    }

    public static class TestScenarios
    {
        public static Dictionary<string, object> CreateProductCacheEntries()
        {
            return new Dictionary<string, object>
            {
                ["product:123"] = new { Id = Guid.NewGuid(), Name = "Test Product 1" },
                ["product:456"] = new { Id = Guid.NewGuid(), Name = "Test Product 2" },
                ["product:789"] = new { Id = Guid.NewGuid(), Name = "Test Product 3" }
            };
        }

        public static Dictionary<string, object> CreateCategoryCacheEntries()
        {
            return new Dictionary<string, object>
            {
                ["category:abc"] = new { Id = Guid.NewGuid(), Name = "Electronics" },
                ["category:def"] = new { Id = Guid.NewGuid(), Name = "Clothing" },
                ["category:ghi"] = new { Id = Guid.NewGuid(), Name = "Books" }
            };
        }

        public static Dictionary<string, object> CreateUserCacheEntries()
        {
            return new Dictionary<string, object>
            {
                ["user:profile:1"] = new { Id = 1, Email = "user1@test.com" },
                ["user:profile:2"] = new { Id = 2, Email = "user2@test.com" },
                ["user:session:token123"] = new { UserId = 1, ExpiresAt = DateTime.UtcNow.AddHours(1) }
            };
        }

        public static Dictionary<string, object> CreateMixedCacheEntries()
        {
            var entries = new Dictionary<string, object>();
            
            // Add different types of cache entries
            var productEntries = CreateProductCacheEntries();
            var categoryEntries = CreateCategoryCacheEntries();
            var userEntries = CreateUserCacheEntries();

            foreach (var entry in productEntries) entries[entry.Key] = entry.Value;
            foreach (var entry in categoryEntries) entries[entry.Key] = entry.Value;
            foreach (var entry in userEntries) entries[entry.Key] = entry.Value;

            return entries;
        }
    }
}