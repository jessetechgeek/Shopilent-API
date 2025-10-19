namespace Shopilent.Infrastructure.Payments.Settings;

public class StripeSettings
{
    public const string SectionName = "Stripe";
    
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2025-06-30";
    public bool EnableTestMode { get; set; } = true;
}