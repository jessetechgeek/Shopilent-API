using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.Search.Meilisearch.Settings;
using Shopilent.Infrastructure.Search.Meilisearch.Services;

namespace Shopilent.API.IntegrationTests.Common;

public class ApiIntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly ApiIntegrationTestFixture _fixture = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test configuration and environment for TestContainers
        builder.UseConfiguration(_fixture.Configuration);
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext configuration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            // Add test database context using TestContainer
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_fixture.PostgreSqlContainer.GetConnectionString()));

            // Configure Redis cache to use TestContainer
            services.Configure<RedisCacheOptions>(redisCacheOptions =>
                redisCacheOptions.Configuration = _fixture.RedisContainer.GetConnectionString());

            // Remove existing Meilisearch service configuration and re-add with test container settings
            services.RemoveAll(typeof(ISearchService));
            services.RemoveAll(typeof(MeilisearchService));
            services.RemoveAll(typeof(IOptions<MeilisearchSettings>));
            services.RemoveAll(typeof(IOptionsSnapshot<MeilisearchSettings>));
            services.RemoveAll(typeof(IOptionsMonitor<MeilisearchSettings>));

            // Configure Meilisearch settings for test container
            var meilisearchSettings = new MeilisearchSettings
            {
                Url = $"http://localhost:{_fixture.MeilisearchContainer.GetMappedPublicPort(7700)}",
                ApiKey = "test-master-key",
                Indexes = new MeilisearchIndexes
                {
                    Products = "products_api_test"
                },
                BatchSize = 100
            };

            services.AddSingleton<IOptions<MeilisearchSettings>>(new OptionsWrapper<MeilisearchSettings>(meilisearchSettings));

            // Re-add Meilisearch service with test configuration
            services.AddScoped<ISearchService, MeilisearchService>();
        });
    }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public ApiIntegrationTestFixture Fixture => _fixture;
}