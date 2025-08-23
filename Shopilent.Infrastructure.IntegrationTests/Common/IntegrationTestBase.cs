using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Extensions;
using Shopilent.Infrastructure.Cache.Redis.Extensions;
using Shopilent.Infrastructure.Extensions;
using Shopilent.Infrastructure.Identity.Extensions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration.Extensions;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Shopilent.Infrastructure.S3ObjectStorage.Extensions;
using Shopilent.Infrastructure.Search.Meilisearch.Extensions;
using Shopilent.Infrastructure.Payments.Extensions;
using StackExchange.Redis;

namespace Shopilent.Infrastructure.IntegrationTests.Common;

public abstract class IntegrationTestBase : IAsyncLifetime, IDisposable
{
    protected readonly IntegrationTestFixture IntegrationTestFixture;

    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;
    protected IConfiguration Configuration => IntegrationTestFixture.Configuration;

    private IServiceScope _scope = null!;

    protected IntegrationTestBase(IntegrationTestFixture integrationTestFixture)
    {
        IntegrationTestFixture = integrationTestFixture;
    }

    public async Task InitializeAsync()
    {
        await ConfigureServices();
        await SeedDatabase();
        await InitializeTestServices();
    }

    public Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    protected virtual Task InitializeTestServices()
    {
        return Task.CompletedTask;
    }

    private async Task ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Configuration);

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Add HTTP context accessor for integration tests
        services.AddHttpContextAccessor();

        // Add Application services (required for MediatR and application handlers)
        services.AddApplicationServices(Configuration);

        // Add core infrastructure services (required for domain events)
        services.AddInfrastructureServices(Configuration);

        // Add Identity services (required for AuditSaveChangesInterceptor)
        services.AddIdentityServices(Configuration);

        // Use the full PostgreSQL persistence services
        services.AddPostgresPersistence(Configuration);

        // Add Redis cache services
        services.AddCacheServices(Configuration);

        // Add storage services (for S3/MinIO integration)
        services.AddStorageServices(Configuration);

        // Add MeiliSearch services
        services.AddMeilisearch(Configuration);

        // Add Payment services
        services.AddPaymentServices(Configuration);

        // Add MediatR for the current assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IntegrationTestBase).Assembly);
        });

        // Add mock user context for integration tests to enable audit interceptor
        var testUserId = Guid.NewGuid();
        var mockUserContext = new Mock<ICurrentUserContext>();
        mockUserContext.Setup(x => x.UserId).Returns(testUserId);
        mockUserContext.Setup(x => x.Email).Returns("test@integrationtest.com");
        mockUserContext.Setup(x => x.IpAddress).Returns("127.0.0.1");
        mockUserContext.Setup(x => x.UserAgent).Returns("Integration Test Browser/1.0");
        mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        mockUserContext.Setup(x => x.IsInRole(It.IsAny<string>())).Returns((string role) => role == "TestRole");
        services.AddSingleton(mockUserContext.Object);

        ServiceProvider = services.BuildServiceProvider();
        _scope = ServiceProvider.CreateScope();

        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // CacheService = _scope.ServiceProvider.GetRequiredService<ICacheService>();

        await DbContext.Database.MigrateAsync();
    }

    private async Task SeedDatabase()
    {
        // Base seeding can be done here or in individual test classes
        await DbContext.SaveChangesAsync();
    }

    protected async Task ResetDatabaseAsync()
    {
        await IntegrationTestFixture.ResetDatabaseAsync();
    }

    protected async Task ClearCacheAsync()
    {
        var connectionMultiplexer = _scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var database = connectionMultiplexer.GetDatabase();
        await database.ExecuteAsync("FLUSHDB");
    }

    protected T GetService<T>() where T : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<T>();
    }

    protected T? GetOptionalService<T>() where T : class
    {
        return _scope.ServiceProvider.GetService<T>();
    }

    public void Dispose()
    {
        _scope?.Dispose();
        if (ServiceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }
}
