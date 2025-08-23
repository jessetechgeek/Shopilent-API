using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Shopilent.Infrastructure.IntegrationTests.Common;

public class IntegrationTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgreSqlContainer { get; }
    public RedisContainer RedisContainer { get; }
    public MinioContainer MinioContainer { get; }
    public IContainer MeilisearchContainer { get; }
    
    public IConfiguration Configuration { get; private set; } = null!;
    private Respawner _respawner = null!;

    public IntegrationTestFixture()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("shopilent_integration_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithCleanUp(true)
            .Build();

        RedisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        MinioContainer = new MinioBuilder()
            .WithImage("minio/minio:latest")
            .WithUsername("minioadmin")
            .WithPassword("minioadmin123")
            .WithCleanUp(true)
            .Build();

        MeilisearchContainer = new ContainerBuilder()
            .WithImage("getmeili/meilisearch:v1.10")
            .WithPortBinding(7700, true)
            .WithEnvironment("MEILI_MASTER_KEY", "test-master-key")
            .WithEnvironment("MEILI_ENV", "development")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(7700).ForPath("/health")))
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Check if running in CI environment (GitHub Actions)
        var isRunningInCI = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
        Console.WriteLine($"[DEBUG] IsRunningInCI: {isRunningInCI}");
        Console.WriteLine($"[DEBUG] GITHUB_ACTIONS env var: {Environment.GetEnvironmentVariable("GITHUB_ACTIONS")}");
        
        if (!isRunningInCI)
        {
            Console.WriteLine("[DEBUG] Starting testcontainers for local development");
            // Start testcontainers for local development
            await PostgreSqlContainer.StartAsync();
            await RedisContainer.StartAsync();
            await MinioContainer.StartAsync();
            await MeilisearchContainer.StartAsync();
        }
        else
        {
            Console.WriteLine("[DEBUG] Using GitHub Actions service containers");
        }

        await ConfigureSettings();
        await InitializeDatabase();
        await InitializeRespawner();
    }

    private Task ConfigureSettings()
    {
        var isRunningInCI = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
        
        var configValues = new Dictionary<string, string?>();
        
        if (isRunningInCI)
        {
            // Use GitHub Actions service containers
            configValues = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=shopilent_integration_test;Username=postgres;Password=postgres",
                ["ConnectionStrings:PostgreSql"] = "Host=localhost;Port=5432;Database=shopilent_integration_test;Username=postgres;Password=postgres",
                ["ConnectionStrings:PostgreSqlReadReplicas"] = "",
                ["Redis:ConnectionString"] = "localhost:6379",
                ["Redis:InstanceName"] = "IntegrationTest",
                ["MinIO:Endpoint"] = "localhost:9000",
                ["MinIO:AccessKey"] = "minioadmin",
                ["MinIO:SecretKey"] = "minioadmin123",
                ["MinIO:BucketName"] = "test-bucket",
                ["MinIO:UseSSL"] = "false",
                ["S3:Provider"] = "MinIO",
                ["S3:AccessKey"] = "minioadmin",
                ["S3:SecretKey"] = "minioadmin123",
                ["S3:Region"] = "us-east-1",
                ["S3:DefaultBucket"] = "test-bucket",
                ["S3:ServiceUrl"] = "http://localhost:9000",
                ["S3:ForcePathStyle"] = "true",
                ["S3:UseSsl"] = "false",
                ["Meilisearch:Url"] = "http://localhost:7700",
                ["Meilisearch:ApiKey"] = "test-master-key",
                ["Meilisearch:Indexes:Products"] = "products_test",
                ["Meilisearch:BatchSize"] = "100",
                ["Seq:ServerUrl"] = null, // Disable Seq in CI
            };
        }
        else
        {
            // Use testcontainers for local development
            configValues = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = PostgreSqlContainer.GetConnectionString(),
                ["ConnectionStrings:PostgreSql"] = PostgreSqlContainer.GetConnectionString(),
                ["ConnectionStrings:PostgreSqlReadReplicas"] = "",
                ["Redis:ConnectionString"] = RedisContainer.GetConnectionString(),
                ["Redis:InstanceName"] = "IntegrationTest",
                ["MinIO:Endpoint"] = MinioContainer.GetConnectionString(),
                ["MinIO:AccessKey"] = "minioadmin",
                ["MinIO:SecretKey"] = "minioadmin123",
                ["MinIO:BucketName"] = "test-bucket",
                ["MinIO:UseSSL"] = "false",
                ["S3:Provider"] = "MinIO",
                ["S3:AccessKey"] = "minioadmin",
                ["S3:SecretKey"] = "minioadmin123",
                ["S3:Region"] = "us-east-1",
                ["S3:DefaultBucket"] = "test-bucket",
                ["S3:ServiceUrl"] = MinioContainer.GetConnectionString(),
                ["S3:ForcePathStyle"] = "true",
                ["S3:UseSsl"] = "false",
                ["Meilisearch:Url"] = $"http://localhost:{MeilisearchContainer.GetMappedPublicPort(7700)}",
                ["Meilisearch:ApiKey"] = "test-master-key",
                ["Meilisearch:Indexes:Products"] = "products_test",
                ["Meilisearch:BatchSize"] = "100",
                ["Seq:ServerUrl"] = null, // Disable Seq in integration tests
            };
        }
        
        // Common configuration for both environments
        var commonConfig = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "test-jwt-key-for-integration-tests-with-minimum-256-bits-length",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:TokenLifetimeMinutes"] = "30",
            ["Jwt:RefreshTokenLifetimeDays"] = "7",
            ["Email:SenderEmail"] = "test@shopilent.com",
            ["Email:SenderName"] = "Shopilent Test",
            ["Email:SmtpServer"] = "localhost",
            ["Email:SmtpPort"] = "587",
            ["Email:SmtpUsername"] = "testuser",
            ["Email:SmtpPassword"] = "testpass",
            ["Email:EnableSsl"] = "false",
            ["Email:SendEmails"] = "false",
            ["Email:AppUrl"] = "https://test.shopilent.com",
            ["Outbox:ProcessingIntervalSeconds"] = "30",
            ["Outbox:MaxRetryAttempts"] = "3",
            ["Outbox:BatchSize"] = "10",
            ["Stripe:SecretKey"] = "sk_test_integration_test_key",
            ["Stripe:PublishableKey"] = "pk_test_integration_test_key",
            ["Stripe:WebhookSecret"] = "whsec_test_integration_webhook_secret",
            ["Stripe:ApiVersion"] = "2025-06-30",
            ["Stripe:EnableTestMode"] = "true"
        };
        
        // Merge environment-specific and common configuration
        foreach (var kvp in commonConfig)
        {
            configValues[kvp.Key] = kvp.Value;
        }

        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues);

        Configuration = configurationBuilder.Build();
        return Task.CompletedTask;
    }

    private async Task InitializeDatabase()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"[DEBUG] Database connection string: {connectionString}");
        
        var services = new ServiceCollection();
        services.AddSingleton(Configuration);
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine("[DEBUG] Running database migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("[DEBUG] Database migrations completed");
        
        // Check if tables exist
        try
        {
            var tableCount = await dbContext.Database.ExecuteSqlRawAsync("SELECT 1"); // Simple connection test
            Console.WriteLine("[DEBUG] Database connection successful");
            
            // Get table count using a proper query
            using var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'";
            var count = await command.ExecuteScalarAsync();
            Console.WriteLine($"[DEBUG] Number of tables found: {count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error checking tables: {ex.Message}");
        }
    }

    private async Task InitializeRespawner()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"],
            DbAdapter = DbAdapter.Postgres
        });
    }

    public async Task ResetDatabaseAsync()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
        await RedisContainer.DisposeAsync();
        await MinioContainer.DisposeAsync();
        await MeilisearchContainer.DisposeAsync();
    }
}