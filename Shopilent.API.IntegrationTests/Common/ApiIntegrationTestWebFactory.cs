using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

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