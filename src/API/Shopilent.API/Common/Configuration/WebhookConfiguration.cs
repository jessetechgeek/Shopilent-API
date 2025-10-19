namespace Shopilent.API.Common.Configuration;

public class WebhookConfiguration
{
    public const string SectionName = "Webhooks";

    public Dictionary<string, WebhookProviderConfig> Providers { get; set; } = new();
}

public class WebhookProviderConfig
{
    public string Token { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}