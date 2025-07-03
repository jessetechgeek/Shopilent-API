using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Settings.Outbox;

namespace Shopilent.Infrastructure.BackgroundServices.Outbox;

public class OutboxProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxProcessingService> _logger;
    private readonly OutboxSettings _settings;
    private Timer _cleanupTimer;

    public OutboxProcessingService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<OutboxSettings> settings,
        ILogger<OutboxProcessingService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processing service is starting");

        // Setup cleanup timer
        _cleanupTimer = new Timer(async _ => await CleanupOldMessages(stoppingToken), 
            null, 
            TimeSpan.Zero, 
            TimeSpan.FromHours(_settings.CleanupIntervalHours));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_settings.ProcessingIntervalMilliseconds, stoppingToken);
        }

        _logger.LogInformation("Outbox processing service is stopping");
    }

    private async Task ProcessOutboxMessages(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
        await outboxService.ProcessMessagesAsync(stoppingToken);
    }

    private async Task CleanupOldMessages(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
            await outboxService.CleanupOldMessagesAsync(_settings.DaysToKeepProcessedMessages, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up old outbox messages");
        }
    }

    public override void Dispose()
    {
        _cleanupTimer?.Dispose();
        base.Dispose();
    }
}