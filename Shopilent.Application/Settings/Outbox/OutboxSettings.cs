namespace Shopilent.Application.Settings.Outbox;

public class OutboxSettings
{
    public int ProcessingIntervalMilliseconds { get; set; } = 5000;
    public int DaysToKeepProcessedMessages { get; set; } = 7;
    public int CleanupIntervalHours { get; set; } = 24;
    public int MaxRetryCount { get; set; } = 5;
}