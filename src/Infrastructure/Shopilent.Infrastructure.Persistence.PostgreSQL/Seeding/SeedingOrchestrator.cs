using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding;

public class SeedingOrchestrator : ISeedingOrchestrator
{
    private readonly IEnumerable<IDataSeeder> _seeders;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedingOrchestrator> _logger;
    private readonly SeedingSettings _options;

    public SeedingOrchestrator(
        IEnumerable<IDataSeeder> seeders,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SeedingOrchestrator> logger,
        IOptions<SeedingSettings> options)
    {
        _seeders = seeders;
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IEnumerable<SeedingResult>> SeedAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<SeedingResult>();
        var overallStopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("=== Starting Database Seeding ===");
            _logger.LogInformation("Configuration - CleanBeforeSeed: {CleanBeforeSeed}", _options.CleanBeforeSeed);

            // Clean database if configured
            if (_options.CleanBeforeSeed)
            {
                await CleanDatabaseAsync(cancellationToken);
            }

            // Execute seeders in order
            var orderedSeeders = _seeders.OrderBy(s => s.Order).ToList();
            _logger.LogInformation("Executing {Count} seeders in sequence", orderedSeeders.Count);

            foreach (var seeder in orderedSeeders)
            {
                _logger.LogInformation("--- Executing {SeederName} ---", seeder.SeederName);
                var result = await seeder.SeedAsync(cancellationToken);
                results.Add(result);

                if (result.Success)
                {
                    _logger.LogInformation("{SeederName} completed successfully - {RecordCount} records in {Duration}ms",
                        seeder.SeederName, result.RecordsCreated, result.Duration.TotalMilliseconds);

                    // Save changes after each seeder
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    _logger.LogError(result.Exception, "{SeederName} failed: {Message}",
                        seeder.SeederName, result.Message);
                    // Continue with other seeders even if one fails
                }
            }

            overallStopwatch.Stop();

            // Log summary
            LogSeedingSummary(results, overallStopwatch.Elapsed);

            return results;
        }
        catch (Exception ex)
        {
            overallStopwatch.Stop();
            _logger.LogError(ex, "Fatal error during database seeding");
            results.Add(SeedingResult.FailureResult("SeedingOrchestrator", ex));
            return results;
        }
    }

    private async Task CleanDatabaseAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Cleaning database - All data will be deleted!");

        try
        {
            // Disable foreign key constraints
            await _context.Database.ExecuteSqlRawAsync(
                "SET session_replication_role = 'replica';", cancellationToken);

            // Truncate tables in reverse dependency order
            var tables = new[]
            {
                "outbox_messages",
                "audit_logs",
                "payments",
                "payment_methods",
                "order_items",
                "orders",
                "cart_items",
                "carts",
                "variant_attributes",
                "product_variants",
                "product_attributes",
                "product_categories",
                "products",
                "attributes",
                "categories",
                "addresses",
                "refresh_tokens",
                "users"
            };

            foreach (var table in tables)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    $"TRUNCATE TABLE {table} RESTART IDENTITY CASCADE;", cancellationToken);
            }

            // Re-enable foreign key constraints
            await _context.Database.ExecuteSqlRawAsync(
                "SET session_replication_role = 'origin';", cancellationToken);

            _logger.LogInformation("Database cleaned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning database");
            throw;
        }
    }

    private void LogSeedingSummary(List<SeedingResult> results, TimeSpan totalDuration)
    {
        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);
        var totalRecords = results.Sum(r => r.RecordsCreated);

        _logger.LogInformation("=== Seeding Summary ===");
        _logger.LogInformation("Total Seeders Executed: {TotalCount}", results.Count);
        _logger.LogInformation("Successful: {SuccessCount}", successCount);
        _logger.LogInformation("Failed: {FailureCount}", failureCount);
        _logger.LogInformation("Total Records Created: {TotalRecords}", totalRecords);
        _logger.LogInformation("Total Duration: {Duration}ms ({Seconds}s)",
            totalDuration.TotalMilliseconds, totalDuration.TotalSeconds);

        if (successCount > 0)
        {
            _logger.LogInformation("--- Successful Seeders ---");
            foreach (var result in results.Where(r => r.Success))
            {
                _logger.LogInformation("  ✓ {SeederName}: {RecordCount} records ({Duration}ms)",
                    result.SeederName, result.RecordsCreated, result.Duration.TotalMilliseconds);
            }
        }

        if (failureCount > 0)
        {
            _logger.LogWarning("--- Failed Seeders ---");
            foreach (var result in results.Where(r => !r.Success))
            {
                _logger.LogWarning("  ✗ {SeederName}: {Message}",
                    result.SeederName, result.Message);
            }
        }

        _logger.LogInformation("=== Seeding Complete ===");
    }
}
