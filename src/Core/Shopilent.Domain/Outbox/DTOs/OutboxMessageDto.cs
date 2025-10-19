namespace Shopilent.Domain.Outbox.DTOs;

public class OutboxMessageDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string Error { get; set; }
    public int RetryCount { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public bool IsProcessed => ProcessedAt.HasValue;
    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsScheduled => ScheduledAt.HasValue && ScheduledAt > DateTime.UtcNow;
}